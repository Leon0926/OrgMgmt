using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>
/// Associating and de-associating services with clients and with employees.
///
/// Clients and services are many-to-many, so a service may be shared by several clients.
/// An employee owns a service (Service.EmployeeID), so assigning a service to an employee
/// moves it away from whoever held it before.
/// </summary>
public sealed class ServiceAssociationTests : UITestBase
{
    public ServiceAssociationTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void Services_can_be_associated_with_a_client_when_it_is_created()
    {
        CreateService("Tax Filing", 120m);
        CreateService("Bookkeeping", 85m);
        CreateService("Payroll", 150m);

        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name("Karen Knight")
            .Address("11 Aspen Place")
            .Balance(500m)
            .PickDateOfBirth(new DateTime(1993, 2, 14))
            .SelectServices("Tax Filing", "Payroll")
            .Submit();

        WaitForPath("/Clients");

        var index = IndexPage.ForPeople(Driver);
        Assert.Equal(
            new[] { "Payroll", "Tax Filing" },
            ServicesOf(index.Cell("Karen Knight", "services")));

        index.ClickDetails("Karen Knight");
        Assert.Equal(
            new[] { "Payroll", "Tax Filing" },
            ServicesOf(new DetailsPage(Driver).Field("services")));
    }

    [Fact]
    public void A_service_can_be_de_associated_from_a_client()
    {
        CreateService("Tax Filing", 120m);
        CreateService("Bookkeeping", 85m);
        CreateClientWithServices("Karen Knight", "Tax Filing", "Bookkeeping");

        GoTo("/Clients");
        IndexPage.ForPeople(Driver).ClickEdit("Karen Knight");

        // Keep only one of the two.
        new PersonFormPage(Driver).SelectServices("Bookkeeping").Submit();
        WaitForPath("/Clients");

        Assert.Equal(
            new[] { "Bookkeeping" },
            ServicesOf(IndexPage.ForPeople(Driver).Cell("Karen Knight", "services")));
    }

    [Fact]
    public void Services_can_be_associated_with_an_employee_and_then_de_associated()
    {
        CreateService("Tax Filing", 120m);
        CreateService("Bookkeeping", 85m);

        GoTo("/Employees/Create");
        new PersonFormPage(Driver)
            .Name("Liam Lowe")
            .Address("13 Hazel Row")
            .Salary(58000m)
            .PickDateOfBirth(new DateTime(1986, 4, 6))
            .SelectServices("Tax Filing", "Bookkeeping")
            .Submit();
        WaitForPath("/Employees");

        Assert.Equal(
            new[] { "Bookkeeping", "Tax Filing" },
            ServicesOf(IndexPage.ForPeople(Driver).Cell("Liam Lowe", "services")));

        // The service's own index should now name the employee.
        GoTo("/Services");
        Assert.Equal("Liam Lowe", IndexPage.ForServices(Driver).Cell("Tax Filing", "employee"));

        // De-associate one of them.
        GoTo("/Employees");
        IndexPage.ForPeople(Driver).ClickEdit("Liam Lowe");
        new PersonFormPage(Driver).SelectServices("Bookkeeping").Submit();
        WaitForPath("/Employees");

        Assert.Equal(
            new[] { "Bookkeeping" },
            ServicesOf(IndexPage.ForPeople(Driver).Cell("Liam Lowe", "services")));

        GoTo("/Services");
        Assert.Equal("", IndexPage.ForServices(Driver).Cell("Tax Filing", "employee"));
    }

    /// <summary>Splits a comma-separated cell into a sorted list, so order never matters.</summary>
    private static string[] ServicesOf(string cellText) => cellText
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .OrderBy(s => s, StringComparer.Ordinal)
        .ToArray();

    private void CreateService(string type, decimal rate)
    {
        GoTo("/Services/Create");
        new ServiceFormPage(Driver).Type(type).Rate(rate).Submit();
        WaitForPath("/Services");
    }

    private void CreateClientWithServices(string name, params string[] serviceTypes)
    {
        GoTo("/Clients/Create");
        new PersonFormPage(Driver)
            .Name(name)
            .Address("11 Aspen Place")
            .Balance(500m)
            .PickDateOfBirth(new DateTime(1993, 2, 14))
            .SelectServices(serviceTypes)
            .Submit();
        WaitForPath("/Clients");
    }
}
