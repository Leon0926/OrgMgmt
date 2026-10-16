using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>
/// CRUD UI automation for Clients, following the four scenarios in the assignment.
/// Note that Person.Name rejects digits and caps at 20 characters, so the fixtures
/// below use plain alphabetic names.
/// </summary>
public sealed class ClientCrudTests : UITestBase
{
    public ClientCrudTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void Creating_a_client_shows_it_on_the_index()
    {
        GoTo("/Clients/Create");

        new PersonFormPage(Driver)
            .Name("Alice Anderson")
            .Address("12 Maple Street")
            .Balance(150.00m)
            .PickDateOfBirth(new DateTime(1990, 5, 15))
            .Submit();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.True(index.Has("Alice Anderson"), "The new client is missing from the index.");
        Assert.Equal("12 Maple Street", index.Cell("Alice Anderson", "address"));
        Assert.Equal("1990-05-15", index.Cell("Alice Anderson", "dateOfBirth"));
        Assert.Equal(150.00m, decimal.Parse(index.Cell("Alice Anderson", "balance")));
    }

    [Fact]
    public void Creating_three_clients_shows_all_three_on_the_index()
    {
        var expected = new[]
        {
            ("Alice Anderson", "12 Maple Street",  100.00m, new DateTime(1990, 5, 15)),
            ("Bruno Bauer",    "34 Oak Avenue",    250.50m, new DateTime(1985, 11, 2)),
            ("Clara Chen",     "56 Pine Road",     375.25m, new DateTime(2000, 1, 30)),
        };

        foreach (var (name, address, balance, dob) in expected)
        {
            CreateClient(name, address, balance, dob);
        }

        GoTo("/Clients");
        var index = IndexPage.ForPeople(Driver);

        Assert.Equal(3, index.RowCount());
        foreach (var (name, address, balance, dob) in expected)
        {
            Assert.True(index.Has(name), $"'{name}' is missing from the index.");
            Assert.Equal(address, index.Cell(name, "address"));
            Assert.Equal(dob.ToString("yyyy-MM-dd"), index.Cell(name, "dateOfBirth"));
            Assert.Equal(balance, decimal.Parse(index.Cell(name, "balance")));
        }
    }

    [Fact]
    public void Editing_the_second_client_shows_the_new_values_on_its_details_page()
    {
        CreateClient("Alice Anderson", "12 Maple Street", 100.00m, new DateTime(1990, 5, 15));
        CreateClient("Bruno Bauer",    "34 Oak Avenue",   250.50m, new DateTime(1985, 11, 2));
        CreateClient("Clara Chen",     "56 Pine Road",    375.25m, new DateTime(2000, 1, 30));

        // Edit the second client from the index view.
        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickEdit("Bruno Bauer");

        new PersonFormPage(Driver)
            .Name("Bruno Baumann")
            .Address("99 Birch Lane")
            .Balance(999.99m)
            .PickDateOfBirth(new DateTime(1986, 12, 24))
            .Submit();

        WaitForPath("/Clients");

        // Verify on that client's details view.
        var index = IndexPage.ForPeople(Driver);
        Assert.False(index.Has("Bruno Bauer"), "The old name is still listed after the edit.");
        index.ClickDetails("Bruno Baumann");

        var details = new DetailsPage(Driver);
        Assert.Equal("Bruno Baumann", details.Field("name"));
        Assert.Equal("99 Birch Lane", details.Field("address"));
        Assert.Equal("1986-12-24", details.Field("dateOfBirth"));
        Assert.Equal(999.99m, decimal.Parse(details.Field("balance")));

        // The other two were left alone.
        GoTo("/Clients");
        Assert.True(IndexPage.ForPeople(Driver).Has("Alice Anderson"));
        Assert.True(IndexPage.ForPeople(Driver).Has("Clara Chen"));
    }

    [Fact]
    public void Deleting_the_third_client_removes_it_from_the_index()
    {
        CreateClient("Alice Anderson", "12 Maple Street", 100.00m, new DateTime(1990, 5, 15));
        CreateClient("Bruno Bauer",    "34 Oak Avenue",   250.50m, new DateTime(1985, 11, 2));
        CreateClient("Clara Chen",     "56 Pine Road",    375.25m, new DateTime(2000, 1, 30));

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickDelete("Clara Chen");
        new DeletePage(Driver).Confirm();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.False(index.Has("Clara Chen"), "The deleted client is still listed.");
        Assert.True(index.Has("Alice Anderson"));
        Assert.True(index.Has("Bruno Bauer"));
        Assert.Equal(2, index.RowCount());
    }

    private void CreateClient(string name, string address, decimal balance, DateTime dateOfBirth)
    {
        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name(name)
            .Address(address)
            .Balance(balance)
            .PickDateOfBirth(dateOfBirth)
            .Submit();
        WaitForPath("/Clients");
    }
}
