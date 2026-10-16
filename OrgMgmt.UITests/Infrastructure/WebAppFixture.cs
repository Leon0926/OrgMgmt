using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Data.Sqlite;

namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Hosts the OrgMgmt web application once for the whole test run, against a throwaway
/// SQLite database so the developer's own app.db is never touched.
/// </summary>
public sealed class WebAppFixture : IAsyncLifetime
{
    private Process? _app;
    private string _dbPath = string.Empty;

    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var solutionDir = FindSolutionDirectory();
        var webProject = Path.Combine(solutionDir, "OrgMgmt", "OrgMgmt.csproj");
        if (!File.Exists(webProject))
        {
            throw new InvalidOperationException($"Could not find the web project at {webProject}.");
        }

        _dbPath = Path.Combine(Path.GetTempPath(), $"orgmgmt-uitests-{Guid.NewGuid():N}.db");
        var port = GetFreePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        var startInfo = new ProcessStartInfo("dotnet")
        {
            // --no-launch-profile keeps launchSettings.json from overriding the URL and environment.
            Arguments = $"run --project \"{webProject}\" --no-launch-profile --urls {BaseUrl}",
            WorkingDirectory = solutionDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={_dbPath}";

        _app = Process.Start(startInfo)
               ?? throw new InvalidOperationException("Failed to start the web application.");

        // Drain the pipes so a chatty log can never fill the buffer and stall the app.
        _ = _app.StandardOutput.ReadToEndAsync();
        _ = _app.StandardError.ReadToEndAsync();

        await WaitUntilRespondingAsync();
    }

    public Task DisposeAsync()
    {
        if (_app is { HasExited: false })
        {
            // dotnet run launches the app as a child process, so the whole tree must go.
            _app.Kill(entireProcessTree: true);
            _app.WaitForExit(10_000);
        }
        _app?.Dispose();

        SqliteConnection.ClearAllPools();
        foreach (var path in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" })
        {
            try { File.Delete(path); } catch (IOException) { /* best effort */ }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Empties every table so each test class starts from a known, countable state.
    /// Child tables go first because of the foreign keys.
    /// </summary>
    public void ResetDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "DELETE FROM ClientService; " +
            "DELETE FROM Services; " +
            "DELETE FROM People;";
        command.ExecuteNonQuery();
    }

    private async Task WaitUntilRespondingAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var deadline = DateTime.UtcNow.AddSeconds(120);

        while (DateTime.UtcNow < deadline)
        {
            if (_app is { HasExited: true })
            {
                throw new InvalidOperationException(
                    $"The web application exited with code {_app.ExitCode} before it became ready.");
            }

            try
            {
                var response = await client.GetAsync($"{BaseUrl}/Clients");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Not listening yet.
            }
            catch (TaskCanceledException)
            {
                // Still warming up.
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"The web application did not respond at {BaseUrl} within 120 seconds.");
    }

    /// <summary>
    /// Walks up from the test assembly to the directory that holds the web project. Anchoring
    /// on the .csproj keeps this working whichever solution format (.sln or .slnx) is in use.
    /// </summary>
    private static string FindSolutionDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null &&
               !File.Exists(Path.Combine(dir.FullName, "OrgMgmt", "OrgMgmt.csproj")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException(
            "Could not locate OrgMgmt/OrgMgmt.csproj in any directory above the test assembly.");
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
