using OrgMgmt.UITests.Infrastructure;
using OrgMgmt.UITests.Pages;

namespace OrgMgmt.UITests.Tests;

/// <summary>CRUD UI automation for Employees, mirroring the four Client scenarios.</summary>
public sealed class EmployeeCrudTests : UITestBase
{
    public EmployeeCrudTests(WebAppFixture app) : base(app) { }

    [Fact]
    public void Creating_an_employee_shows_it_on_the_index()
    {
        GoTo("/Employees/Create");

        new PersonFormPage(Driver)
            .Name("Dana Dixon")
            .Address("78 Cedar Court")
            .Salary(55000m)
            .PickDateOfBirth(new DateTime(1988, 3, 9))
            .Submit();

        WaitForPath("/Employees");

        var index = IndexPage.ForPeople(Driver);
        Assert.True(index.Has("Dana Dixon"), "The new employee is missing from the index.");
        Assert.Equal("78 Cedar Court", index.Cell("Dana Dixon", "address"));
        Assert.Equal("1988-03-09", index.Cell("Dana Dixon", "dateOfBirth"));
        Assert.Equal(55000m, decimal.Parse(index.Cell("Dana Dixon", "salary")));
    }

    [Fact]
    public void Creating_three_employees_shows_all_three_on_the_index()
    {
        var expected = new[]
        {
            ("Dana Dixon",   "78 Cedar Court", 55000m, new DateTime(1988, 3, 9)),
            ("Evan Edwards", "90 Elm Way",     62000m, new DateTime(1979, 7, 21)),
            ("Fiona Flores", "21 Spruce Lane", 48500m, new DateTime(1995, 10, 4)),
        };

        foreach (var (name, address, salary, dob) in expected)
        {
            CreateEmployee(name, address, salary, dob);
        }

        GoTo("/Employees");
        var index = IndexPage.ForPeople(Driver);

        Assert.Equal(3, index.RowCount());
        foreach (var (name, address, salary, dob) in expected)
        {
            Assert.True(index.Has(name), $"'{name}' is missing from the index.");
            Assert.Equal(address, index.Cell(name, "address"));
            Assert.Equal(dob.ToString("yyyy-MM-dd"), index.Cell(name, "dateOfBirth"));
            Assert.Equal(salary, decimal.Parse(index.Cell(name, "salary")));
        }
    }

    [Fact]
    public void Editing_the_second_employee_shows_the_new_values_on_its_details_page()
    {
        CreateEmployee("Dana Dixon",   "78 Cedar Court", 55000m, new DateTime(1988, 3, 9));
        CreateEmployee("Evan Edwards", "90 Elm Way",     62000m, new DateTime(1979, 7, 21));
        CreateEmployee("Fiona Flores", "21 Spruce Lane", 48500m, new DateTime(1995, 10, 4));

        GoTo("/Employees");
        IndexPage.ForPeople(Driver).ClickEdit("Evan Edwards");

        new PersonFormPage(Driver)
            .Name("Evan Edmunds")
            .Address("15 Willow Street")
            .Salary(71500m)
            .PickDateOfBirth(new DateTime(1980, 2, 29))
            .Submit();

        WaitForPath("/Employees");

        var index = IndexPage.ForPeople(Driver);
        Assert.False(index.Has("Evan Edwards"), "The old name is still listed after the edit.");
        index.ClickDetails("Evan Edmunds");

        var details = new DetailsPage(Driver);
        Assert.Equal("Evan Edmunds", details.Field("name"));
        Assert.Equal("15 Willow Street", details.Field("address"));
        Assert.Equal("1980-02-29", details.Field("dateOfBirth"));
        Assert.Equal(71500m, decimal.Parse(details.Field("salary")));
    }

    [Fact]
    public void Deleting_the_third_employee_removes_it_from_the_index()
    {
        CreateEmployee("Dana Dixon",   "78 Cedar Court", 55000m, new DateTime(1988, 3, 9));
        CreateEmployee("Evan Edwards", "90 Elm Way",     62000m, new DateTime(1979, 7, 21));
        CreateEmployee("Fiona Flores", "21 Spruce Lane", 48500m, new DateTime(1995, 10, 4));

        GoTo("/Employees");
        IndexPage.ForPeople(Driver).ClickDelete("Fiona Flores");
        new DeletePage(Driver).Confirm();

        WaitForPath("/Employees");

        var index = IndexPage.ForPeople(Driver);
        Assert.False(index.Has("Fiona Flores"), "The deleted employee is still listed.");
        Assert.True(index.Has("Dana Dixon"));
        Assert.True(index.Has("Evan Edwards"));
        Assert.Equal(2, index.RowCount());
    }

    private void CreateEmployee(string name, string address, decimal salary, DateTime dateOfBirth)
    {
        GoTo("/Employees/Create");
        new PersonFormPage(Driver)
            .Name(name)
            .Address(address)
            .Salary(salary)
            .PickDateOfBirth(dateOfBirth)
            .Submit();
        WaitForPath("/Employees");
    }
}
