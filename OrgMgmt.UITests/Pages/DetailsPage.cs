using OpenQA.Selenium;

namespace OrgMgmt.UITests.Pages;

/// <summary>A Details view, read through the data-field attributes on its dd elements.</summary>
public sealed class DetailsPage
{
    private readonly IWebDriver _driver;

    public DetailsPage(IWebDriver driver) => _driver = driver;

    public string Field(string field) =>
        _driver.FindElement(By.CssSelector($"dd[data-field='{field}']")).Text.Trim();

    public bool HasPhoto() => _driver.FindElements(By.Id("currentPhoto")).Count > 0;

    public bool ShowsNoPhoto() => _driver.FindElements(By.Id("noPhoto")).Count > 0;

    /// <summary>The photo's resolved URL, for asserting the image really is served.</summary>
    public string PhotoUrl() => _driver.FindElement(By.Id("currentPhoto")).GetAttribute("src") ?? "";
}
