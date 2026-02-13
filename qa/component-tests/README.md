# Component Tests (NUnit)

Place NUnit component tests for API endpoints in:

- `CalendarTasking.ComponentTests`

Scope plan:

1. CRUD endpoints first (`Users`, `Calendars`, `Events`, `Tasks`, `PrivateClassSessions`)
2. Then custom operations (`login`, `password change`, `mark paid/unpaid`, `monthly summary`, etc.)

CRUD templates (3 placeholders per operation) are scaffolded in:

- `CalendarTasking.ComponentTests/Templates/UsersCrudTemplateTests.cs`
- `CalendarTasking.ComponentTests/Templates/CalendarsCrudTemplateTests.cs`
- `CalendarTasking.ComponentTests/Templates/EventsCrudTemplateTests.cs`
- `CalendarTasking.ComponentTests/Templates/TasksCrudTemplateTests.cs`
- `CalendarTasking.ComponentTests/Templates/PrivateClassSessionsCrudTemplateTests.cs`

Current count:

- `25` CRUD operations
- `20` implemented tests (`1` per CRUD action per resource)
- `55` placeholders still marked `[Ignore]`

Each placeholder test is marked with `[Ignore]`.
Workflow:

1. Pick one placeholder test.
2. Implement arrange/act/assert.
3. Remove `[Ignore]` from that test.
