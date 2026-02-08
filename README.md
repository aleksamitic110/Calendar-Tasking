# Calendar-Tasking

Backend API and SQL database for a QA class project: calendar, events, tasks, and private class tracking (including payment status).

## Tech Stack

- .NET 9 Web API
- Entity Framework Core 9 (SQL Server)
- SQL Server schema scripts for manual setup

## Domain Entities

- `Users`
- `Calendars`
- `Events`
- `Tasks`
- `PrivateClassSessions`

## Database Setup

1. Open SQL Server Management Studio.
2. Run `database/schema.sql`.
3. Optional: run `database/seed.sql`.

Default seeded login:

- Email: `ana@example.com`
- Password: `Pass123!`

## API Setup

1. Update connection string in `src/CalendarTasking.Api/appsettings.json` if needed.
2. Restore and run:

```powershell
dotnet restore CalendarTasking.sln
dotnet run --project src/CalendarTasking.Api/CalendarTasking.Api.csproj
```

API docs/testing in development:

- Swagger UI: `http://localhost:5170/swagger`
- `GET /openapi/v1.json`
- Vue user app: `http://localhost:5170/`
- Vue API lab: `http://localhost:5170/api-lab.html`

You can also test with the VS Code REST Client using:

- `src/CalendarTasking.Api/CalendarTasking.Api.http`

## Frontend (Vue Client)

- Client source is in `client/`.
- The backend serves this folder directly, so running the API also serves the client.
- End-user app is `client/index.html` + `client/main.js` + `client/styles.css` (register/login + calendars/tasks/events/private sessions).
- API testing panel is preserved in `client/api-lab.html` + `client/api-lab.js` + `client/api-lab.css`.

## Main Endpoints

### Users

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users/register`
- `POST /api/users/login`
- `PUT /api/users/{id}`
- `PUT /api/users/{id}/password`
- `DELETE /api/users/{id}`

### Calendars

- `GET /api/calendars`
- `GET /api/calendars/{id}`
- `POST /api/calendars`
- `PUT /api/calendars/{id}`
- `DELETE /api/calendars/{id}`

### Events

- `GET /api/events`
- `GET /api/events/{id}`
- `POST /api/events`
- `PUT /api/events/{id}`
- `DELETE /api/events/{id}`

### Tasks

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PUT /api/tasks/{id}/status`
- `DELETE /api/tasks/{id}`

### Private Class Sessions

- `GET /api/private-class-sessions`
- `GET /api/private-class-sessions/{id}`
- `GET /api/private-class-sessions/unpaid`
- `GET /api/private-class-sessions/monthly-summary?calendarId={id}&year={yyyy}&month={mm}`
- `POST /api/private-class-sessions`
- `PUT /api/private-class-sessions/{id}`
- `PUT /api/private-class-sessions/{id}/mark-paid`
- `PUT /api/private-class-sessions/{id}/mark-unpaid`
- `DELETE /api/private-class-sessions/{id}`
