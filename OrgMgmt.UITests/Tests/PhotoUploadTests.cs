using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>
/// Upload, replace and remove of Client and Employee photos.
///
/// Selenium performs a file upload by sending the file's absolute path to the
/// &lt;input type="file"&gt; element. The browser's native file-chooser dialog is never
/// opened; WebDriver has no way to drive an OS-level dialog, and this path-based
/// approach is the documented and supported technique.
/// </summary>
public sealed class PhotoUploadTests : UITestBase
{
    public PhotoUploadTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void Uploading_a_photo_when_creating_a_client_shows_it_afterwards()
    {
        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name("Gina Grant")
            .Address("3 Rose Terrace")
            .Balance(80m)
            .PickDateOfBirth(new DateTime(1992, 6, 18))
            .UploadPhoto(TestImage.CreateRed())
            .Submit();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.True(index.RowHasPhoto("Gina Grant"), "The index shows no photo for the new client.");

        index.ClickDetails("Gina Grant");
        var details = new DetailsPage(Driver);
        Assert.True(details.HasPhoto(), "The details view shows no photo.");

        // The image is really served, not just an img tag pointing at nothing.
        var bytes = Download(details.PhotoUrl());
        Assert.NotEmpty(bytes);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes.Take(4));
    }

    [Fact]
    public void Replacing_a_client_photo_stores_the_new_image()
    {
        CreateClientWithPhoto("Gina Grant", TestImage.CreateRed());

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickDetails("Gina Grant");
        var original = Download(new DetailsPage(Driver).PhotoUrl());

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickEdit("Gina Grant");

        var form = new PersonFormPage(Driver);
        Assert.True(form.ShowsCurrentPhoto(), "The edit form does not preview the existing photo.");
        form.UploadPhoto(TestImage.CreateBlue()).Submit();

        WaitForPath("/Clients");
        IndexPage.ForPeople(Driver).ClickDetails("Gina Grant");

        var details = new DetailsPage(Driver);
        Assert.True(details.HasPhoto(), "The photo disappeared after replacing it.");

        var replaced = Download(details.PhotoUrl());
        Assert.NotEqual(original, replaced);
    }

    [Fact]
    public void Removing_a_client_photo_clears_it()
    {
        CreateClientWithPhoto("Gina Grant", TestImage.CreateRed());

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickEdit("Gina Grant");
        new PersonFormPage(Driver).RemovePhoto().Submit();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.False(index.RowHasPhoto("Gina Grant"), "The index still shows a photo after removal.");

        index.ClickDetails("Gina Grant");
        var details = new DetailsPage(Driver);
        Assert.False(details.HasPhoto(), "The details view still shows a photo after removal.");
        Assert.True(details.ShowsNoPhoto(), "The details view does not report the photo as absent.");
    }

    [Fact]
    public void Uploading_replacing_and_removing_an_employee_photo()
    {
        GoTo("/Employees/Create");
        new PersonFormPage(Driver)
            .Name("Harry Hobbs")
            .Address("8 Tulip Drive")
            .Salary(50000m)
            .PickDateOfBirth(new DateTime(1983, 9, 1))
            .UploadPhoto(TestImage.CreateRed())
            .Submit();
        WaitForPath("/Employees");

        Assert.True(IndexPage.ForPeople(Driver).RowHasPhoto("Harry Hobbs"),
            "The employee photo was not stored on create.");

        // Replace.
        IndexPage.ForPeople(Driver).ClickDetails("Harry Hobbs");
        var original = Download(new DetailsPage(Driver).PhotoUrl());

        GoTo("/Employees");
        IndexPage.ForPeople(Driver).ClickEdit("Harry Hobbs");
        new PersonFormPage(Driver).UploadPhoto(TestImage.CreateBlue()).Submit();
        WaitForPath("/Employees");

        IndexPage.ForPeople(Driver).ClickDetails("Harry Hobbs");
        var replaced = Download(new DetailsPage(Driver).PhotoUrl());
        Assert.NotEqual(original, replaced);

        // Remove.
        GoTo("/Employees");
        IndexPage.ForPeople(Driver).ClickEdit("Harry Hobbs");
        new PersonFormPage(Driver).RemovePhoto().Submit();
        WaitForPath("/Employees");

        Assert.False(IndexPage.ForPeople(Driver).RowHasPhoto("Harry Hobbs"),
            "The employee photo survived removal.");
    }

    private void CreateClientWithPhoto(string name, string photoPath)
    {
        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name(name)
            .Address("3 Rose Terrace")
            .Balance(80m)
            .PickDateOfBirth(new DateTime(1992, 6, 18))
            .UploadPhoto(photoPath)
            .Submit();
        WaitForPath("/Clients");
    }
}
