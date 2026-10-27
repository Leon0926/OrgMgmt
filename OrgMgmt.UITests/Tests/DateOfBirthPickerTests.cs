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
    public void A_date_picked_from_the_calendar_is_saved_for_a_client()
    {
        var date = new DateTime(1990, 5, 15);

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
}
