namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Writes small real PNG files to disk for the upload tests. Selenium uploads by sending the
/// file's path to the file input, so the bytes must actually exist on the file system.
/// </summary>
public static class TestImage
{
    // A 1x1 PNG. The two variants differ in content so a "replace" can be proven.
    private static readonly byte[] RedPixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGP4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

    private static readonly byte[] BluePixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGNgYPgPAAEDAQAIicLsAAAAAElFTkSuQmCC");

    public static string CreateRed() => Write(RedPixel, "red");

    public static string CreateBlue() => Write(BluePixel, "blue");

    private static string Write(byte[] bytes, string label)
    {
        var dir = Path.Combine(Path.GetTempPath(), "orgmgmt-uitest-images");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"{label}-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(path, bytes);
        return path;
    }
}
