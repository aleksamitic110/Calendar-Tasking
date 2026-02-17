# Component Tests (NUnit)

NUnit component tests for API endpoints are in:

- `CalendarTasking.ComponentTests`

Coverage status:

- `32` API operations covered
- `96` NUnit component tests total (`3` per operation)
- `75` CRUD tests in `Templates/`
- `21` custom operation tests in `Custom/`

Folders:

- `CalendarTasking.ComponentTests/Templates` (`Users`, `Calendars`, `Events`, `Tasks`, `PrivateClassSessions` CRUD)
- `CalendarTasking.ComponentTests/Custom` (`login`, `password`, `task status`, `unpaid`, `monthly summary`, `mark-paid`, `mark-unpaid`)

Run only component tests:

```powershell
dotnet test .\component-tests\CalendarTasking.ComponentTests\CalendarTasking.ComponentTests.csproj
```

Run only CRUD template tests:

```powershell
dotnet test .\component-tests\CalendarTasking.ComponentTests\CalendarTasking.ComponentTests.csproj --filter "FullyQualifiedName~Templates"
```

Run only custom operation tests:

```powershell
dotnet test .\component-tests\CalendarTasking.ComponentTests\CalendarTasking.ComponentTests.csproj --filter "FullyQualifiedName~Custom"
```
