# Backend

ASP.NET Core Web API for Enterprise AI Document Assistant.

## Current Scope

- Controller-based API project
- Swagger UI
- Health check endpoint
- Status endpoint
- CORS for the React development server
- ProblemDetails and simple global error handling

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Basic service health check |
| GET | `/api/status` | API status, version, environment, and AI provider placeholder |
| GET | `/swagger` | Swagger UI in development |

## Commands

```bash
dotnet build EnterpriseAiDocumentAssistant.sln
dotnet run --project src/EnterpriseAiDocumentAssistant.Api
```
