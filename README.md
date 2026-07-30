# Enterprise AI Document Assistant

A production-oriented React + ASP.NET Core application for connecting the core building blocks of modern AI applications: assistant UI, prompt orchestration, AI Gateway, document processing, controlled skills, Tool Gateway, MCP surface, and workflow orchestration.

V1 is intentionally small: one end-to-end document assistant flow, implemented in clear steps. Retrieval supports both an in-memory vector store and persistent Qdrant behind the same application interface.

---

## V1 Architecture

```mermaid
flowchart LR
    user[User] --> ui[React Workspace]

    subgraph frontend[Frontend]
        ui --> docs[Document View]
        ui --> assistant[Right-side Assistant]
        ui --> panels[Citations + Tool Results]
    end

    assistant --> api[ASP.NET Core API]
    docs --> api

    subgraph backend[Backend]
        api --> gateway[AI Gateway]
        api --> tools[Tool Gateway]
        api --> workflows[Workflow API]
        gateway --> prompts[Prompt Orchestration]
        api --> rag[RAG Retrieval]
        rag --> embeddings[Embedding Gateway]
        rag --> vectors[Qdrant / In-memory Vector Store]
        workflows --> planner[AI Planner + Fallback]
    end

    subgraph ai[AI + Extension Points]
        gateway --> models[OpenAI / Azure OpenAI]
        tools --> mcp[MCP Surface]
        tools --> msgraph[Microsoft Graph Adapter]
    end
```

---

## Current Core Flow

```mermaid
sequenceDiagram
    participant U as User
    participant UI as React Workspace
    participant API as ASP.NET Core API
    participant AI as AI Gateway
    participant Model as OpenAI / Azure OpenAI / Mock

    U->>UI: Ask about a document
    UI->>API: POST /api/chat/stream
    API->>API: Safety classifier + guardrails + conversation memory
    API->>AI: Build orchestrated prompt
    AI->>Model: Generate structured answer
    Model-->>AI: JSON-shaped response
    AI-->>API: Validated answer
    API-->>UI: Stream response
```

---

## V1 Modules

| Module | Purpose | First Scope |
|---|---|---|
| React Workspace | User-facing work area | Document list, document workspace tabs, right-side Assistant, citations, tool results |
| ASP.NET Core API | Backend boundary | `/api/chat`, `/api/documents`, `/api/tools`, `/api/workflows` |
| Prompt and AI Layer | Controlled model behavior | Prompt orchestration, structured output, validation, safety classifier, guardrails, AI Gateway |
| Tool Gateway and Skills | Controlled actions | `GetHealthStatusTool`, `GetDocumentMetadataTool`, `SummarySkill`, `RiskAnalysisSkill`, `EmailDraftSkill`, `ResumeReviewSkill` |
| Document Processing and RAG | Source-grounded answers | Upload, parse, chunk, embedding, Qdrant vector search, citations |
| Persistence | Application state | MongoDB document records and Qdrant vector indexes |
| MCP / Harness / Workflow / Integration | Extension path | MCP wrapper over existing tools, prompt/tool harnesses, one workflow, Microsoft Graph adapter boundary |

---

## Current Status

### Completed Core

- [x] React workspace with document list, upload zone, document tabs, and right-side Assistant
- [x] ASP.NET Core controller API with Swagger, contracts, and ProblemDetails
- [x] Backend-driven workspace data and document upload flow
- [x] Text parsing, chunking, and document preview
- [x] Chat endpoint with prompt orchestration, conversation memory, structured output, safety classifier, and guardrails
- [x] AI Gateway with local mock, OpenAI, and Azure OpenAI provider selection
- [x] Skills: classification, summary, risk analysis, email draft, and resume review
- [x] Workflow: document summary -> risk analysis -> email draft
- [x] Tool Gateway with health and document metadata tools
- [x] MCP controller surface over registered tools
- [x] In-memory audit logging
- [x] AI intent routing through Agent Planner with deterministic fallback
- [x] MongoDB document persistence for uploaded document metadata and parsed sections
- [x] Input Guardrails with rule-based safety classification, optional AI-backed safety classification, and deterministic fallback
- [x] RAG baseline with embedding gateway, configurable in-memory/Qdrant vector store, semantic retrieval, and retrieved chunk citations
- [x] RAG no-answer threshold with controlled insufficient-evidence responses
- [x] Qdrant vector persistence behind `IVectorStore`, with provider-specific collections

### Lightweight Boundaries

- [x] Microsoft Graph adapter scaffold with mock email draft output, not OAuth-backed real Graph calls
- [x] Prompt and tool harness checks for basic regression coverage

### Not Built Yet

- [ ] MongoDB persistence for conversation, workflow, and audit storage
- [ ] LLM-native function/tool calling loop
- [ ] Real Microsoft Graph OAuth integration
- [ ] Basic document permission filtering

### Build Next

- [ ] Conversation storage with MongoDB or relational storage
- [ ] Citation display review
- [ ] Basic document permission filtering

### Build Lightly

- [ ] Rate limiting
- [ ] Observability and cost tracking
- [ ] Prompt versioning
- [ ] Sensitive data redaction for AI logs
- [ ] Expanded harness checks for prompts, skills, tools, and workflows
- [ ] Simple Agent Orchestration / A2A handoff

---

## Next Implementation Order

```text
Persistence
  -> Citation Display Review
  -> Basic Document Permission Filtering
  -> Rate Limiting
  -> Observability and Cost Tracking
  -> Prompt Versioning
  -> Sensitive Data Redaction
  -> Expanded Harness Checks
  -> Simple Agent Orchestration / A2A Handoff
```

`Build Next` items form the main delivery path. `Build Lightly` items stay intentionally small and are implemented only when they strengthen the application architecture.

### Deferred Scope

- Hybrid search and semantic ranking
- Real Microsoft Graph OAuth integration
- GraphQL API surface
- CI and deployment hardening

---

## Tech Stack

| Area | Stack |
|---|---|
| Frontend | React, TypeScript, Vite, Tailwind CSS |
| Backend | ASP.NET Core Web API |
| AI | OpenAI / Azure OpenAI, Semantic Kernel or Microsoft.Extensions.AI friendly design |
| Retrieval | Embeddings, vector store, source citations |
| Persistence | MongoDB document records, Qdrant vectors, in-memory development alternatives |
| Integration | Microsoft Graph, REST APIs, MCP |

---

## Local Development

```bash
git clone https://github.com/haoyucheng369-gif/enterprise-ai-document-assistant.git
cd enterprise-ai-document-assistant
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Backend local AI provider settings can be placed in:

```text
backend/src/EnterpriseAiDocumentAssistant.Api/appsettings.Local.json
```

Edit that local file with your provider, model, and API key. The local file is ignored by Git.

Local MongoDB and Qdrant can be started from the repository root:

```bash
docker compose up -d mongodb qdrant
docker compose ps
```

MongoDB Compass connection string:

```text
mongodb://localhost:27017
```

Qdrant dashboard:

```text
http://localhost:6333/dashboard
```

Both databases use Docker named volumes, so data survives container restarts. Use `docker compose down -v` only when you intentionally want to remove the local data.

---

## Documentation

- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)
- [Chinese README](README.zh-CN.md)

