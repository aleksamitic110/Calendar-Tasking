# QA Structure

This folder is intentionally separated from `src/` and `client/` so QA work can be delivered and run independently.

## Projects

- `component-tests/CalendarTasking.ComponentTests`
- `playwright-tests/CalendarTasking.PlaywrightTests`

Both projects are included in `CalendarTasking.QA.sln`.

## Assignment Mapping

- `component-tests`: NUnit component tests for API operations (start with CRUD, then extend to custom endpoints).
- `playwright-tests`: Playwright for .NET end-to-end tests and API tests.

## Suggested Workflow

1. Start the app stack with Docker from repository root:
   - `copy .env.example .env`
   - `docker compose up --build`
2. Run QA tests separately from this folder:
   - `dotnet restore CalendarTasking.QA.sln`
   - `dotnet test CalendarTasking.QA.sln`

For Playwright .NET browser binaries (one-time setup per machine):
- `pwsh .\playwright-tests\CalendarTasking.PlaywrightTests\bin\Debug\net9.0\playwright.ps1 install`
