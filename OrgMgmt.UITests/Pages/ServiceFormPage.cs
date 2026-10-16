using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OrgMgmt.UITests.Infrastructure;

namespace OrgMgmt.UITests.Pages;

/// <summary>The Create / Edit form for a Service.</summary>
public sealed class ServiceFormPage
{
    private readonly IWebDriver _driver;

    public ServiceFormPage(IWebDriver driver) => _driver = driver;

    public ServiceFormPage Type(string value) => Fill("Type", value);

    public ServiceFormPage Rate(decimal value) => Fill("Rate", value.ToString());

    /// <summary>Assigns the service to an employee by name, or to nobody when null.</summary>
    public ServiceFormPage AssignEmployee(string? employeeName)
    {
        var select = new SelectElement(_driver.FindElement(By.Id("EmployeeID")));
        if (employeeName == null)
        {
            select.SelectByIndex(0);
        }
        else
        {
            select.SelectByText(employeeName);
        }
        return this;
    }

    /// <summary>Associates exactly these clients, de-associating anything else selected.</summary>
    public ServiceFormPage SelectClients(params string[] clientNames)
    {
        var select = new SelectElement(_driver.FindElement(By.Id("selectedClients")));
        select.DeselectAll();
        foreach (var name in clientNames)
        {
            select.SelectByText(name);
        }
        Slowmo.Pause();
        return this;
    }

    public IReadOnlyList<string> SelectedClients()
    {
        var select = new SelectElement(_driver.FindElement(By.Id("selectedClients")));
        return select.AllSelectedOptions.Select(o => o.Text).ToList();
    }

    public void Submit()
    {
        Slowmo.Pause();
        _driver.FindElement(By.CssSelector("input[type=submit]")).Click();
    }

    private ServiceFormPage Fill(string id, string value)
    {
        var field = _driver.FindElement(By.Id(id));
        field.Clear();
        field.SendKeys(value);
        Slowmo.Pause();
        return this;
    }
}
