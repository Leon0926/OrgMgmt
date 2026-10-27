using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Data.Sqlite;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Hosts the OrgMgmt web application once for the whole test run, against a throwaway
/// SQLite database so the developer's own app.db is never touched. Also owns the one Chrome
/// instance that every test shares.
/// </summary>
public sealed class WebAppFixture : IAsyncLifetime
{
    private Process? _app;
    private string _dbPath = string.Empty;

    public string BaseUrl { get; private set; } = string.Empty;

    public IWebDriver Driver { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var solutionDir = FindSolutionDirectory();
        var webProjectDir = Path.Combine(solutionDir, "OrgMgmt");
        var webProject = Path.Combine(webProjectDir, "OrgMgmt.csproj");
        if (!File.Exists(webProject))
        {
            throw new InvalidOperationException($"Could not find the web project at {webProject}.");
        }

        _dbPath = Path.Combine(Path.GetTempPath(), $"orgmgmt-uitests-{Guid.NewGuid():N}.db");
        var port = GetFreePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        // The test project references the web project, so it is already built. Running that
        // build directly skips the second compile that "dotnet run" would do.
        var webDll = FindBuiltWebApp(webProjectDir);
        var arguments = webDll != null
            ? $"\"{webDll}\" --urls {BaseUrl}"
            // --no-launch-profile keeps launchSettings.json from overriding the URL and environment.
            : $"run --project \"{webProject}\" --no-launch-profile --urls {BaseUrl}";

        var startInfo = new ProcessStartInfo("dotnet")
        {
            Arguments = arguments,
            // The project folder is the content root, so wwwroot and the views resolve.
            WorkingDirectory = webProjectDir,
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

        Driver = StartBrowser();
    }

    public Task DisposeAsync()
    {
        try
        {
            Driver?.Quit();
        }
        catch (WebDriverException)
        {
            // The browser may already be gone; nothing useful to do here.
        }
        Driver?.Dispose();

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

    private static IWebDriver StartBrowser()
    {
        var options = new ChromeOptions();
        if (Environment.GetEnvironmentVariable("ORGMGMT_UITESTS_HEADED") != "1")
        {
            options.AddArgument("--headless=new");
        }
        options.AddArgument("--lang=en-US");
        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--disable-extensions");
        options.AddUserProfilePreference("intl.accept_languages", "en-US");

        return new ChromeDriver(options);
    }

    /// <summary>
    /// The web project's build output for the same configuration and framework as this test
    /// assembly (e.g. bin/Debug/net10.0/OrgMgmt.dll), or null if it is not there.
    /// </summary>
    private static string? FindBuiltWebApp(string webProjectDir)
    {
        var testBin = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        var framework = testBin.Name;
        var configuration = testBin.Parent?.Name;
        if (configuration == null)
        {
            return null;
        }

        var dll = Path.Combine(webProjectDir, "bin", configuration, framework, "OrgMgmt.dll");
        return File.Exists(dll) ? dll : null;
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
