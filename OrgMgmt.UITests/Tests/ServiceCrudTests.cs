using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>CRUD UI automation for Services, mirroring the four Client scenarios.</summary>
public sealed class ServiceCrudTests : UITestBase
{
    public ServiceCrudTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void Creating_a_service_shows_it_on_the_index()
    {
        GoTo("/Services/Create");

        new ServiceFormPage(Driver)
            .Type("Tax Filing")
            .Rate(120.00m)
            .Submit();

        WaitForPath("/Services");

        var index = IndexPage.ForServices(Driver);
        Assert.True(index.Has("Tax Filing"), "The new service is missing from the index.");
        Assert.Equal(120.00m, decimal.Parse(index.Cell("Tax Filing", "rate")));
    }

    [Fact]
    public void Creating_three_services_shows_all_three_on_the_index()
    {
        var expected = new[]
        {
            ("Tax Filing",   120.00m),
            ("Bookkeeping",   85.50m),
            ("Payroll",      150.75m),
        };

        foreach (var (type, rate) in expected)
        {
            CreateService(type, rate);
        }

        GoTo("/Services");
        var index = IndexPage.ForServices(Driver);

        Assert.Equal(3, index.RowCount());
        foreach (var (type, rate) in expected)
        {
            Assert.True(index.Has(type), $"'{type}' is missing from the index.");
            Assert.Equal(rate, decimal.Parse(index.Cell(type, "rate")));
        }
    }

    [Fact]
    public void Editing_the_second_service_shows_the_new_values_on_its_details_page()
    {
        CreateService("Tax Filing", 120.00m);
        CreateService("Bookkeeping", 85.50m);
        CreateService("Payroll", 150.75m);

        GoTo("/Services");
        IndexPage.ForServices(Driver).ClickEdit("Bookkeeping");

        new ServiceFormPage(Driver)
            .Type("Monthly Bookkeeping")
            .Rate(99.95m)
            .Submit();

        WaitForPath("/Services");

        var index = IndexPage.ForServices(Driver);
        Assert.False(index.Has("Bookkeeping"), "The old service type is still listed after the edit.");
        index.ClickDetails("Monthly Bookkeeping");

        var details = new DetailsPage(Driver);
        Assert.Equal("Monthly Bookkeeping", details.Field("type"));
        Assert.Equal(99.95m, decimal.Parse(details.Field("rate")));
    }

    [Fact]
    public void Deleting_the_third_service_removes_it_from_the_index()
    {
        CreateService("Tax Filing", 120.00m);
        CreateService("Bookkeeping", 85.50m);
        CreateService("Payroll", 150.75m);

        GoTo("/Services");
        IndexPage.ForServices(Driver).ClickDelete("Payroll");
        new DeletePage(Driver).Confirm();

        WaitForPath("/Services");

        var index = IndexPage.ForServices(Driver);
        Assert.False(index.Has("Payroll"), "The deleted service is still listed.");
        Assert.True(index.Has("Tax Filing"));
        Assert.True(index.Has("Bookkeeping"));
        Assert.Equal(2, index.RowCount());
    }

    private void CreateService(string type, decimal rate)
    {
        GoTo("/Services/Create");
        new ServiceFormPage(Driver).Type(type).Rate(rate).Submit();
        WaitForPath("/Services");
    }
}
