# Backend

ASP.NET Core Web API for Enterprise AI Document Assistant.

## Current Scope

- Controller-based API project
- Swagger UI
- Health check endpoint
- Status endpoint
- Workspace seed endpoint
- Mock chat endpoint
- Streaming mock chat endpoint
- Prompt orchestration service for template variables and response rules
- CORS for the React development server
- ProblemDetails and simple global error handling

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Basic service health check |
| GET | `/api/status` | API status, version, environment, and AI provider placeholder |
| GET | `/api/workspace` | Initial workspace data for the React shell |
| POST | `/api/chat` | Mock assistant response for request/response chat flow |
| POST | `/api/chat/stream` | Mock assistant response streamed as text chunks |
| GET | `/swagger` | Swagger UI in development |

## Commands

```bash
dotnet build EnterpriseAiDocumentAssistant.sln
dotnet run --project src/EnterpriseAiDocumentAssistant.Api
```
