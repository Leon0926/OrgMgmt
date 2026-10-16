using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>
/// Automation of the date-of-birth calendar widget: the test opens the picker, chooses the
/// year and month from its header and clicks the day cell, rather than typing a date string.
/// </summary>
public sealed class DateOfBirthPickerTests : UITestBase
{
    public DateOfBirthPickerTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void The_picker_opens_when_the_date_field_is_clicked()
    {
        GoTo("/Clients/Create");

        var form = new PersonFormPage(Driver);
        Assert.False(form.IsDatePickerOpen(), "The calendar was open before anything was clicked.");

        form.PickDateOfBirth(new DateTime(1994, 4, 22));
        Assert.Equal("1994-04-22", Driver.FindElement(OpenQA.Selenium.By.Id("DateOfBirth")).GetAttribute("value"));
    }

    [Theory]
    [InlineData(1975, 1, 1)]     // first day of a month
    [InlineData(1990, 5, 15)]
    [InlineData(2000, 2, 29)]    // leap day
    [InlineData(2004, 12, 31)]   // last day of a year
    public void A_date_picked_from_the_calendar_is_saved_for_a_client(int year, int month, int day)
    {
        var date = new DateTime(year, month, day);

        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name("Iris Ingram")
            .Address("5 Violet Way")
            .Balance(60m)
            .PickDateOfBirth(date)
            .Submit();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.Equal(date.ToString("yyyy-MM-dd"), index.Cell("Iris Ingram", "dateOfBirth"));

        index.ClickDetails("Iris Ingram");
        Assert.Equal(date.ToString("yyyy-MM-dd"), new DetailsPage(Driver).Field("dateOfBirth"));
    }

    [Fact]
    public void A_date_picked_from_the_calendar_is_saved_for_an_employee()
    {
        var date = new DateTime(1987, 8, 7);

        GoTo("/Employees/Create");
        new PersonFormPage(Driver)
            .Name("Jack Jennings")
            .Address("9 Orchid Street")
            .Salary(64000m)
            .PickDateOfBirth(date)
            .Submit();

        WaitForPath("/Employees");

        var index = IndexPage.ForPeople(Driver);
        Assert.Equal(date.ToString("yyyy-MM-dd"), index.Cell("Jack Jennings", "dateOfBirth"));
    }

    [Fact]
    public void Re_picking_a_date_on_the_edit_form_replaces_the_previous_one()
    {
        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name("Iris Ingram")
            .Address("5 Violet Way")
            .Balance(60m)
            .PickDateOfBirth(new DateTime(1990, 5, 15))
            .Submit();
        WaitForPath("/Clients");

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickEdit("Iris Ingram");
        new PersonFormPage(Driver).PickDateOfBirth(new DateTime(1991, 11, 3)).Submit();
        WaitForPath("/Clients");

        Assert.Equal("1991-11-03", IndexPage.ForPeople(Driver).Cell("Iris Ingram", "dateOfBirth"));
    }
}
