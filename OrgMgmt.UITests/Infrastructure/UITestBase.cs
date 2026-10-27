using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// Base class for every UI test: a clean database per test method. The browser itself is
/// shared across the whole run (see <see cref="WebAppFixture"/>), because starting Chrome is
/// the slowest part of each test.
/// Set ORGMGMT_UITESTS_HEADED=1 to watch the tests drive a visible Chrome window.
/// </summary>
[Collection(UiCollection.Name)]
public abstract class UITestBase
{
    protected WebAppFixture App { get; }
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }

    protected UITestBase(WebAppFixture app)
    {
        App = app;
        App.ResetDatabase();

        Driver = App.Driver;
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
    }

    protected void GoTo(string relativeUrl)
        => Driver.Navigate().GoToUrl($"{App.BaseUrl}{relativeUrl}");

    /// <summary>Waits until the browser has landed on the given path, e.g. "/Clients".</summary>
    protected void WaitForPath(string path)
    {
        try
        {
            Wait.Until(d => new Uri(d.Url).AbsolutePath.Equals(path, StringComparison.OrdinalIgnoreCase));
        }
        catch (WebDriverTimeoutException ex)
        {
            // Say where the browser got stuck and why, instead of a bare timeout.
            var errors = Driver.FindElements(By.CssSelector(".text-danger, .validation-summary-errors li"))
                .Select(e => e.Text.Trim())
                .Where(t => t.Length > 0)
                .ToList();
            var detail = errors.Count > 0 ? $" Validation errors: {string.Join("; ", errors)}" : "";
            throw new WebDriverTimeoutException(
                $"Expected to land on {path}, but the browser is still on {Driver.Url}.{detail}", ex);
        }
    }

    /// <summary>Fetches a URL the browser is showing, for asserting on served image bytes.</summary>
    protected static byte[] Download(string url)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
    }
}
