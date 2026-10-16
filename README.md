# OrgMgmt

An ASP.NET Core MVC application for managing an organisation's employees, clients and
services, with a Selenium UI test suite that automates the whole interface.

```
OrgMgmt.slnx          solution
OrgMgmt/              the web application (ASP.NET Core MVC, EF Core, SQLite)
OrgMgmt.UITests/      Selenium UI automation (xUnit)
```

## Running the application

```bash
dotnet run --project OrgMgmt
```

The schema is created on first start by `EnsureCreated()`, into the `app.db` SQLite file
named in `appsettings.json`. There are no EF migrations.

## Running the UI tests

```bash
dotnet test
```

The suite needs **Google Chrome installed**; the matching ChromeDriver is downloaded
automatically by Selenium Manager. Chrome runs headless by default. To watch the browser
work through the scenarios:

```bash
ORGMGMT_UITESTS_HEADED=1 dotnet test
```

`WebAppFixture` starts the web application once per run, on a free loopback port and
against a **throwaway SQLite database** in the temp directory, so your own `app.db` is
never touched. Each test begins by emptying that database, so row counts are predictable.

### Recording a demo of the suite

Headless runs show nothing on screen, and at full speed each test finishes in one to three
seconds. For a screen recording, run headed with a slow-motion delay (milliseconds) and a
logger that prints each test name as it passes:

```bash
ORGMGMT_UITESTS_HEADED=1 ORGMGMT_UITESTS_SLOWMO=350   dotnet test --logger "console;verbosity=detailed"
```

`ORGMGMT_UITESTS_SLOWMO` pauses after each browser interaction; it is off by default so
normal runs stay fast. At 350 ms the full suite takes roughly six minutes. To record one
area at a time, add a filter:

```bash
ORGMGMT_UITESTS_HEADED=1 ORGMGMT_UITESTS_SLOWMO=350   dotnet test --filter "FullyQualifiedName~ClientCrudTests"   --logger "console;verbosity=detailed"
```

Available filters: `ClientCrudTests`, `EmployeeCrudTests`, `ServiceCrudTests`,
`PhotoUploadTests`, `DateOfBirthPickerTests`, `ServiceAssociationTests`.

## What the tests cover

| Area | File |
| --- | --- |
| Client CRUD, incl. three-at-once, edit-the-second, delete-the-third | `Tests/ClientCrudTests.cs` |
| Employee CRUD, same four scenarios | `Tests/EmployeeCrudTests.cs` |
| Service CRUD, same four scenarios | `Tests/ServiceCrudTests.cs` |
| Photo upload, replace, remove (clients and employees) | `Tests/PhotoUploadTests.cs` |
| Picking a date of birth from the calendar widget | `Tests/DateOfBirthPickerTests.cs` |
| Associating and de-associating services with clients and employees | `Tests/ServiceAssociationTests.cs` |

Page objects live in `OrgMgmt.UITests/Pages/`. Views carry `data-field`, `data-name` and
`data-type` attributes so locators do not depend on table column order.

## Two implementation notes

**File uploads.** Selenium uploads a file by sending its absolute path to the
`<input type="file">` element. The browser's native file-chooser dialog is never opened:
WebDriver cannot drive an OS-level dialog, and the path-based approach is the documented
and supported technique. See `PersonFormPage.UploadPhoto`.

**The date picker.** The date of birth fields use the [flatpickr](https://flatpickr.js.org/)
calendar widget (vendored into `wwwroot/lib/flatpickr`) rather than a native
`<input type="date">`. A native date input cannot be driven by WebDriver in current Chrome
— neither typed digits nor arrow keys reach its segments — whereas flatpickr renders a real
DOM calendar. `PersonFormPage.PickDateOfBirth` opens the picker, sets the year and month
from its header and clicks the day cell, which is a genuine "pick from the date picker"
rather than writing a string into a text box.
