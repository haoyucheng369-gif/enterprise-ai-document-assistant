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
- Structured assistant response contract and validation
- Simple chat guardrails for prompt-injection and unauthorized-data requests
- Tool Gateway skeleton with tool listing and execution endpoints
- `get_health_status` tool backed by the API status provider
- `get_document_metadata` tool backed by workspace document data
- CORS for the React development server
- ProblemDetails and simple global error handling

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Basic service health check |
| GET | `/api/status` | API status, version, environment, and AI provider placeholder |
| GET | `/api/workspace` | Initial workspace data for the React shell |
| GET | `/api/tools` | Registered tool definitions |
| POST | `/api/tools/execute` | Execute a registered tool through the Tool Gateway |
| POST | `/api/chat` | Mock assistant response for request/response chat flow |
| POST | `/api/chat/stream` | Mock assistant response streamed as text chunks |
| POST | `/api/chat/structured` | Mock assistant response as validated structured JSON |
| GET | `/swagger` | Swagger UI in development |

## Commands

```bash
dotnet build EnterpriseAiDocumentAssistant.sln
dotnet run --project src/EnterpriseAiDocumentAssistant.Api
```
