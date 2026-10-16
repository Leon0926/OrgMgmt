using OpenQA.Selenium;
using OrgMgmt.UITests.Infrastructure;

namespace OrgMgmt.UITests.Pages;

/// <summary>
/// An entity list view. Rows carry data-id plus a data-name (people) or data-type
/// (services) attribute, and each cell a data-field, so locators do not depend on
/// column order.
/// </summary>
public sealed class IndexPage
{
    private readonly IWebDriver _driver;
    private readonly string _keyAttribute;

    private IndexPage(IWebDriver driver, string keyAttribute)
    {
        _driver = driver;
        _keyAttribute = keyAttribute;
    }

    public static IndexPage ForPeople(IWebDriver driver) => new(driver, "data-name");

    public static IndexPage ForServices(IWebDriver driver) => new(driver, "data-type");

    /// <summary>The key of every row currently listed, in display order.</summary>
    public IReadOnlyList<string> Keys() => _driver
        .FindElements(By.CssSelector($"tbody tr[{_keyAttribute}]"))
        .Select(r => r.GetAttribute(_keyAttribute) ?? string.Empty)
        .ToList();

    public int RowCount() => Keys().Count;

    public bool Has(string key) => Rows(key).Count > 0;

    public IWebElement Row(string key)
    {
        var rows = Rows(key);
        return rows.Count switch
        {
            1 => rows[0],
            0 => throw new NoSuchElementException($"No row with {_keyAttribute}='{key}' on {_driver.Url}."),
            _ => throw new InvalidOperationException($"Expected one row for '{key}' but found {rows.Count}."),
        };
    }

    /// <summary>Reads one cell of a row, e.g. Cell("Ada", "address").</summary>
    public string Cell(string key, string field) =>
        Row(key).FindElement(By.CssSelector($"td[data-field='{field}']")).Text.Trim();

    public bool RowHasPhoto(string key) =>
        Row(key).FindElements(By.CssSelector("img.person-photo")).Count > 0;

    public void ClickEdit(string key) => ClickLink(key, "Edit");

    public void ClickDetails(string key) => ClickLink(key, "Details");

    public void ClickDelete(string key) => ClickLink(key, "Delete");

    public void ClickCreateNew() => _driver.FindElement(By.LinkText("Create New")).Click();

    private void ClickLink(string key, string text)
    {
        Slowmo.Pause();
        Row(key).FindElement(By.LinkText(text)).Click();
    }

    private IReadOnlyList<IWebElement> Rows(string key) =>
        _driver.FindElements(By.CssSelector($"tbody tr[{_keyAttribute}='{EscapeCss(key)}']"));

    private static string EscapeCss(string value) => value.Replace("'", "\\'");
}
