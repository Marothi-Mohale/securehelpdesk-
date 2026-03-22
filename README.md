<<<<<<< HEAD
# Secure Helpdesk API

Production-minded ASP.NET Core Web API portfolio project for secure helpdesk and ticket management.

## Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT authentication
- Swagger / OpenAPI
- xUnit for tests

## Architecture

The solution follows a practical Clean Architecture-inspired split:

- `SecureHelpdesk.Domain`: core entities, enums, and domain constants
- `SecureHelpdesk.Application`: DTOs, service contracts, query models, and business services
- `SecureHelpdesk.Infrastructure`: EF Core, Identity, JWT token generation, persistence, and seeding
- `SecureHelpdesk.Api`: controllers, middleware, dependency injection, and HTTP setup
- `SecureHelpdesk.Tests`: focused unit tests for business rules

## Setup

1. Install the .NET 8 SDK and SQL Server.
2. Update the connection string in `src/SecureHelpdesk.Api/appsettings.json` if needed.
3. Restore packages with `dotnet restore`.
4. Run the API with `dotnet run --project src/SecureHelpdesk.Api`.

The app seeds roles, demo users, and sample tickets on startup. Database creation is handled automatically the first time the API starts.

## Demo Accounts

- `admin@securehelpdesk.local` / `Admin123!`
- `agent1@securehelpdesk.local` / `Agent123!`
- `agent2@securehelpdesk.local` / `Agent123!`
- `user1@securehelpdesk.local` / `User123!`
- `user2@securehelpdesk.local` / `User123!`

## Notes

- Controllers use DTO contracts only; EF entities are not returned directly.
- Business logic is centralized in services instead of duplicated across controllers.
- Global exception handling returns consistent API problem responses and logs failures.
- This environment does not have the .NET SDK installed, so restore, build, and tests still need to be executed locally after installing the SDK.
=======
# securehelpdesk-
Secure Helpdesk API
>>>>>>> 
