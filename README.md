# ItHelpDesk

IT helpdesk ticket system. Employees report IT problems, technicians resolve them, and administrators assign work and manage users.

## Features

- Ticket workflow: New → Assigned → In progress → Resolved → Closed, with reopen and cancel
- Priorities with deadlines and overdue tracking
- Public comments and internal technician notes
- Full status and assignment history
- Role-based access: Employee, Technician, Admin
- User and category management, statistics dashboard
- JWT authentication with rotating refresh tokens

## Tech stack

| Part | Technology |
|---|---|
| API | ASP.NET Core 8 Web API, EF Core, ASP.NET Core Identity, JWT |
| Database | MySQL 8.4 |
| Frontend | React, TypeScript, React Router, Vite |
| Tooling | Docker Compose, Postman |

## Running locally

Requirements: Docker, Node.js 22+.

```bash
docker compose up -d --build

cd ithelpdesk-web
npm install
npm run dev
```

| Service | Address |
|---|---|
| Web app | http://localhost:5173 |
| API | http://localhost:5157 |
| Swagger | http://localhost:5157/swagger |
| MySQL | localhost:3307 |

Development admin account: `admin@helpdesk.lt` / `Admin123!`

## Tests

`postman/` contains an API test collection (396 tests) and an environment. Import both into Postman and run the collection in order.

## Project structure

```
ItHelpDesk.Api/      ASP.NET Core API
  Auth/              login, tokens, sessions, roles
  Controllers/       REST endpoints
  Data/              entities, DTOs, DbContext
  Repositories/      database queries
  Services/          ticket workflow rules
ithelpdesk-web/      React frontend
  src/api/           API client and types
  src/auth/          authentication state
  src/components/    shared components
  src/pages/         pages
postman/             API tests
docker-compose.yml   MySQL + API
```
