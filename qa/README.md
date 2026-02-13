# QA Structure

This folder is intentionally separated from `src/` and `client/` so QA work can be delivered and run independently.

## Projects

- `component-tests/CalendarTasking.ComponentTests`
- `playwright-tests/CalendarTasking.PlaywrightTests`

Both projects are included in `CalendarTasking.QA.sln`.

## Assignment Mapping

- `component-tests`: NUnit component tests for API operations.
- `playwright-tests`: Playwright for .NET end-to-end tests and API tests.

Required by assignment:

1. Web application (already in root project).
2. Component tests in NUnit:
   - for each API operation, write 3 component tests.
   - start from CRUD operations, then include custom operations.
3. End-to-end and API tests in Playwright (.NET).

API operations in this project:

- `Users`: GET all, GET by id, POST register, POST login, PUT update, PUT password, DELETE
- `Calendars`: GET all, GET by id, POST, PUT, DELETE
- `Events`: GET all, GET by id, POST, PUT, DELETE
- `Tasks`: GET all, GET by id, POST, PUT, PUT status, DELETE
- `PrivateClassSessions`: GET all, GET by id, GET unpaid, GET monthly-summary, POST, PUT, PUT mark-paid, PUT mark-unpaid, DELETE

Total operations to eventually cover with 3 NUnit tests each: `32 operations`.

## Suggested Workflow

1. Start the app stack with Docker from repository root:
   - `copy .env.example .env`
   - `docker compose up --build`
2. Run QA tests separately from this folder:
   - `dotnet restore CalendarTasking.QA.sln`
   - `dotnet test CalendarTasking.QA.sln`

Quick runner from repository root:

- `.\run-qa-tests.ps1 -Suite component`
- `.\run-qa-tests.ps1 -Suite component-templates`
- `.\run-qa-tests.ps1 -Suite playwright`
- `.\run-qa-tests.ps1 -Suite all`
- Optional Playwright browser install: `.\run-qa-tests.ps1 -Suite playwright -InstallPlaywright`
- Optional custom base URL: `.\run-qa-tests.ps1 -Suite playwright -BaseUrl http://localhost:5170`

CRUD component templates included:

- `component-tests/CalendarTasking.ComponentTests/Templates/UsersCrudTemplateTests.cs`
- `component-tests/CalendarTasking.ComponentTests/Templates/CalendarsCrudTemplateTests.cs`
- `component-tests/CalendarTasking.ComponentTests/Templates/EventsCrudTemplateTests.cs`
- `component-tests/CalendarTasking.ComponentTests/Templates/TasksCrudTemplateTests.cs`
- `component-tests/CalendarTasking.ComponentTests/Templates/PrivateClassSessionsCrudTemplateTests.cs`

Scaffolded scope:

- CRUD only (`25 operations` across 5 resources)
- `20` implemented NUnit tests (one per CRUD action per resource)
- `55` ignored placeholders still available in templates
- Custom endpoints still need separate templates/tests (`login`, `password`, `task status`, `unpaid`, `monthly-summary`, `mark-paid`, `mark-unpaid`)

Template rule:

- Remaining placeholders are marked with `[Ignore]`.
- Implement test logic and then remove `[Ignore]` for that specific test.

Run only CRUD template tests:

- `dotnet test .\component-tests\CalendarTasking.ComponentTests\CalendarTasking.ComponentTests.csproj --filter "FullyQualifiedName~Templates"`

Playwright setup and run:

1. Build Playwright test project:
   - `dotnet build .\playwright-tests\CalendarTasking.PlaywrightTests\CalendarTasking.PlaywrightTests.csproj`
2. Install browser binaries (one-time):
   - `pwsh .\playwright-tests\CalendarTasking.PlaywrightTests\bin\Debug\net9.0\playwright.ps1 install`
3. Run Playwright test project:
   - `dotnet test .\playwright-tests\CalendarTasking.PlaywrightTests\CalendarTasking.PlaywrightTests.csproj`

Optional base URL override for Playwright tests:

- `$env:CALENDAR_TASKING_BASE_URL="http://localhost:5170"`
