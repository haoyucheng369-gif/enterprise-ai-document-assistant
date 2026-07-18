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
- MCP-style tool listing and call endpoints
- Prompt and tool harness checks
- Summary, risk analysis, and email draft skills
- Conversation memory from recent chat history
- Simple agent planner
- In-memory audit trail
- Mock AI Gateway
- Document upload, text extraction, and chunking
- Document review workflow
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
| GET | `/api/mcp/tools/list` | MCP-style tool descriptors |
| POST | `/api/mcp/tools/call` | MCP-style tool call entry point |
| GET | `/api/harness` | Run prompt, tool, skill, gateway, upload, and workflow checks |
| GET | `/api/audit/events` | Recent audit events |
| GET | `/api/documents/uploads` | Uploaded document metadata |
| POST | `/api/documents/upload` | Upload a supported document |
| POST | `/api/skills/summary` | Run the summary skill |
| POST | `/api/skills/risk-analysis` | Run the risk analysis skill |
| POST | `/api/skills/email-draft` | Run the email draft skill |
| POST | `/api/planner/plan` | Build a deterministic agent plan |
| POST | `/api/workflows/document-review` | Run the document review workflow |
| POST | `/api/chat` | Mock assistant response for request/response chat flow |
| POST | `/api/chat/stream` | Mock assistant response streamed as text chunks |
| POST | `/api/chat/structured` | Mock assistant response as validated structured JSON |
| GET | `/swagger` | Swagger UI in development |

## Commands

```bash
dotnet build EnterpriseAiDocumentAssistant.sln
dotnet run --project src/EnterpriseAiDocumentAssistant.Api
```
