using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OrgMgmt.UITests.Infrastructure;

namespace OrgMgmt.UITests.Pages;

/// <summary>
/// The Create / Edit form shared in shape by Clients and Employees.
/// Every setter returns the page so a test can read as one sentence.
/// </summary>
public sealed class PersonFormPage
{
    private readonly IWebDriver _driver;

    public PersonFormPage(IWebDriver driver) => _driver = driver;

    public PersonFormPage Name(string value) => Fill("Name", value);

    public PersonFormPage Address(string value) => Fill("Address", value);

    public PersonFormPage Balance(decimal value) => Fill("Balance", value.ToString());

    public PersonFormPage Salary(decimal value) => Fill("Salary", value.ToString());

    /// <summary>
    /// Picks a date of birth from the calendar widget the way a user would: open the picker,
    /// choose the year and month from its header, then click the day cell.
    /// </summary>
    public PersonFormPage PickDateOfBirth(DateTime date)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

        var calendar = OpenDatePicker();
        Slowmo.Pause();

        // Year: a numeric field in the picker's header. Clear() is used rather than a
        // select-all chord, which is not portable across operating systems.
        var year = calendar.FindElement(By.CssSelector("input.cur-year"));
        year.Clear();
        year.SendKeys(date.Year.ToString());
        wait.Until(_ => year.GetAttribute("value") == date.Year.ToString());
        Slowmo.Pause();

        // Month: a dropdown in the picker's header.
        var monthDropdown = new SelectElement(
            calendar.FindElement(By.CssSelector("select.flatpickr-monthDropdown-months")));
        monthDropdown.SelectByValue((date.Month - 1).ToString());
        Slowmo.Pause();

        // Day: click the cell inside the displayed month, not the greyed-out neighbours.
        var day = wait.Until(_ => calendar
            .FindElements(By.CssSelector(".flatpickr-day:not(.prevMonthDay):not(.nextMonthDay)"))
            .FirstOrDefault(c => c.Text.Trim() == date.Day.ToString()));
        day!.Click();
        // Let the calendar finish closing so it cannot swallow the next click (e.g. Submit).
        wait.Until(_ => !IsDatePickerOpen());
        Slowmo.Pause();

        var expected = date.ToString("yyyy-MM-dd");
        var input = _driver.FindElement(By.Id("DateOfBirth"));
        wait.Until(_ => input.GetAttribute("value") == expected);

        return this;
    }

    /// <summary>
    /// Clicks the date field and returns the calendar once it is showing. In a visible browser
    /// that is not the focused window, Chrome can drop the focus the click should give the
    /// field, so the calendar never opens; in that case ask the widget to open directly.
    /// </summary>
    private IWebElement OpenDatePicker()
    {
        IWebElement? FindOpenCalendar() =>
            _driver.FindElements(By.CssSelector(".flatpickr-calendar.open")).FirstOrDefault(c => c.Displayed);

        _driver.FindElement(By.Id("DateOfBirth")).Click();

        try
        {
            return new WebDriverWait(_driver, TimeSpan.FromSeconds(3)).Until(_ => FindOpenCalendar())!;
        }
        catch (WebDriverTimeoutException)
        {
            // Fall through and open it directly.
        }

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "document.getElementById('DateOfBirth')._flatpickr.open();");
        return new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(_ => FindOpenCalendar())!;
    }

    /// <summary>True while the calendar widget is open, for asserting the picker really appears.</summary>
    public bool IsDatePickerOpen() =>
        _driver.FindElements(By.CssSelector(".flatpickr-calendar.open")).Any(c => c.Displayed);

    /// <summary>
    /// Uploads a photo by handing the file's path to the file input. This is how Selenium
    /// drives a file upload: the native OS file dialog is never opened, which is both
    /// intentional and the only approach WebDriver supports.
    /// </summary>
    public PersonFormPage UploadPhoto(string absolutePath)
    {
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("The photo to upload does not exist.", absolutePath);
        }

        _driver.FindElement(By.Id("PhotoFile")).SendKeys(absolutePath);
        Slowmo.Pause();
        return this;
    }

    /// <summary>Ticks the "Remove photo" checkbox (Edit form only).</summary>
    public PersonFormPage RemovePhoto()
    {
        var checkbox = _driver.FindElement(By.Id("RemovePhoto"));
        if (!checkbox.Selected)
        {
            checkbox.Click();
        }
        Slowmo.Pause();
        return this;
    }

    /// <summary>Associates exactly these services, de-associating anything else selected.</summary>
    public PersonFormPage SelectServices(params string[] serviceTypes)
    {
        var select = new SelectElement(_driver.FindElement(By.Id("selectedServices")));
        select.DeselectAll();
        foreach (var type in serviceTypes)
        {
            select.SelectByText(type);
        }
        Slowmo.Pause();
        return this;
    }

    /// <summary>De-associates every service.</summary>
    public PersonFormPage ClearServices() => SelectServices();

    public IReadOnlyList<string> SelectedServices()
    {
        var select = new SelectElement(_driver.FindElement(By.Id("selectedServices")));
        return select.AllSelectedOptions.Select(o => o.Text).ToList();
    }

    public bool ShowsCurrentPhoto() => _driver.FindElements(By.Id("currentPhoto")).Count > 0;

    public void Submit()
    {
        Slowmo.Pause();
        _driver.FindElement(By.CssSelector("input[type=submit]")).Click();
    }

    private PersonFormPage Fill(string id, string value)
    {
        var field = _driver.FindElement(By.Id(id));
        field.Clear();
        field.SendKeys(value);
        Slowmo.Pause();
        return this;
    }
}
