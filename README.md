# Calendar Tasking API

This repository contains a comprehensive backend service for a calendar and task management application, built with .NET. It features a full suite of CRUD operations for users, calendars, events, tasks, and private tutoring sessions. The project includes a relational database schema, extensive automated tests, and two distinct frontend clients for demonstration and API testing.

## Core Features
- **User Management**: Secure user registration, login, profile updates, and password changes.
- **Calendars**: Multi-calendar support per user, including default calendar functionality.
- **Events**: Create, read, update, and delete events with filtering by date range. Supports recurring events, reminders, and locations.
- **Tasks**: Manage to-do items with due dates, priorities, and status tracking (`Todo`, `InProgress`, `Done`).
- **Private Class Sessions**: A specialized module for tutors to track student sessions, manage payments, and generate financial summaries. Includes endpoints for marking sessions as paid/unpaid and fetching monthly revenue reports.

## Technology Stack
- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core
- **Database**: MS SQL Server
- **Testing**:
  - **Component Tests**: NUnit, `Microsoft.AspNetCore.Mvc.Testing` for in-memory API testing.
  - **API/E2E Tests**: Playwright for .NET for browser automation and direct API endpoint testing.
- **Frontend**:
  - Vue.js 3 (CDN-based for simplicity)
  - A feature-rich "cyberpunk" themed UI (`index.html`).
  - A developer-focused API testing dashboard (`api-lab.html`).
- **Containerization**: Docker, Docker Compose

## Project Structure
```
/
├── client/              # Contains two standalone frontend applications
├── database/            # SQL scripts for database schema and seed data
├── docker/              # Docker-related scripts (e.g., DB initialization)
├── qa/                  # Quality assurance folder with all test projects
│   ├── component-tests/ # NUnit tests against an in-memory API
│   └── playwright-tests/# Playwright tests for API and UI flows
├── src/                 # Main source code for the ASP.NET Core API
└── docker-compose.yml   # Main file for running the application stack
```

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop

### Running with Docker (Recommended)
This is the simplest way to get the entire application stack—API, database, and clients—running.

1. **(Optional)** Create a `.env` file in the root directory to specify the database password. If this file is omitted, a default password from `docker-compose.yml` will be used.
   ```env
   SA_PASSWORD=YourStrong!Passw0rd
   ```
2. From the root of the repository, run Docker Compose:
   ```sh
   docker-compose up --build
   ```
3. The API will be available at `http://localhost:5170`.
4. The SQL Server database will be accessible on `localhost:14333`.

### Frontend Clients
Once the application is running, you can access the two client applications in your browser:
-   **Main UI**: [http://localhost:5170/index.html](http://localhost:5170/index.html)
-   **API Lab**: [http://localhost:5170/api-lab.html](http://localhost:5170/api-lab.html)

## Running the Test Suites
The repository includes PowerShell scripts to simplify running the various test suites.

1.  **Install Playwright Browsers**
    Before running Playwright tests for the first time, you need to install the required browser binaries.
    ```powershell
    ./run-playwright-tests.ps1 -InstallBrowser
    ```

2.  **Run All QA Tests**
    This command executes both NUnit component tests and Playwright API/UI tests.
    ```powershell
    ./run-qa-tests.ps1 -Suite all
    ```

3.  **Run Component Tests Only**
    ```powershell
    ./run-qa-tests.ps1 -Suite component
    ```

4.  **Run Playwright API & UI Tests Only**
    ```powershell
    ./run-qa-tests.ps1 -Suite playwright
    ```

## Client Applications

### Cyberpunk Dashboard (`index.html`)
A fully functional user interface with a distinct cyberpunk aesthetic. It provides a rich user experience for managing all entities, including:
- Login/Registration.
- CRUD operations for Calendars, Tasks, Events, and Sessions via forms.
- An interactive monthly calendar view.
- A daily timeline view with drag-and-drop support for rescheduling items and resizing durations.
- A quick-access "seed" user for immediate testing: `ana@example.com` / `Pass123!`.

### API Command Deck (`api-lab.html`)
A QA-focused tool that serves as live, interactive API documentation. It allows you to:
- View all available API endpoints, grouped by domain.
- Fill in parameters and request bodies using friendly forms.
- Execute requests against a running API instance.
- Inspect live responses, including status codes, timings, and JSON payloads.

## Database
- **Schema**: The complete database structure is defined in `database/schema.sql`. It includes tables for Users, Calendars, Events, Tasks, and PrivateClassSessions with appropriate constraints and relationships.
- **Seeding**: The `database/seed.sql` script populates the database with initial data, including a test user (`ana@example.com`) and associated sample entries.
- **Initialization**: When using Docker Compose, the `docker/init-db.sh` script automatically waits for the SQL Server instance to be ready and then applies both the schema and seed scripts to initialize the database.
