using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Base class for every UI test: a clean database and a fresh browser per test method.
/// Set ORGMGMT_UITESTS_HEADED=1 to watch the tests drive a visible Chrome window.
/// </summary>
[Collection(UiCollection.Name)]
public abstract class UITestBase : IDisposable
{
    protected WebAppFixture App { get; }
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }

    protected UITestBase(WebAppFixture app)
    {
        App = app;
        App.ResetDatabase();

        var options = new ChromeOptions();
        if (Environment.GetEnvironmentVariable("ORGMGMT_UITESTS_HEADED") != "1")
        {
            options.AddArgument("--headless=new");
        }
        // The date-input tests type digits in MM/DD/YYYY order, so pin the browser locale.
        options.AddArgument("--lang=en-US");
        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddUserProfilePreference("intl.accept_languages", "en-US");

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
    }

    protected void GoTo(string relativeUrl)
        => Driver.Navigate().GoToUrl($"{App.BaseUrl}{relativeUrl}");

    /// <summary>Waits until the browser has landed on the given path, e.g. "/Clients".</summary>
    protected void WaitForPath(string path)
        => Wait.Until(d => new Uri(d.Url).AbsolutePath.Equals(path, StringComparison.OrdinalIgnoreCase));

    /// <summary>Fetches a URL the browser is showing, for asserting on served image bytes.</summary>
    protected static byte[] Download(string url)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        try
        {
            Driver.Quit();
        }
        catch (WebDriverException)
        {
            // The browser may already be gone; nothing useful to do here.
        }
        Driver.Dispose();
    }
}
