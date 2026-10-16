namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Optional pause after each browser interaction, so a screen recording of the suite is
/// watchable. Set ORGMGMT_UITESTS_SLOWMO to a number of milliseconds, e.g. 400.
/// Off by default, so normal runs stay fast.
/// </summary>
public static class Slowmo
{
    private static readonly int DelayMs =
        int.TryParse(Environment.GetEnvironmentVariable("ORGMGMT_UITESTS_SLOWMO"), out var ms) && ms > 0
            ? ms
            : 0;

    public static bool Enabled => DelayMs > 0;

    public static void Pause()
    {
        if (DelayMs > 0)
        {
            Thread.Sleep(DelayMs);
        }
    }
}
