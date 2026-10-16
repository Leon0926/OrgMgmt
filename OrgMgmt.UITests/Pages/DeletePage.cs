using OpenQA.Selenium;
using OrgMgmt.UITests.Infrastructure;

namespace OrgMgmt.UITests.Pages;

/// <summary>The Delete confirmation view.</summary>
public sealed class DeletePage
{
    private readonly IWebDriver _driver;

    public DeletePage(IWebDriver driver) => _driver = driver;

    public void Confirm()
    {
        Slowmo.Pause();
        _driver.FindElement(By.CssSelector("input[type=submit]")).Click();
    }
}
