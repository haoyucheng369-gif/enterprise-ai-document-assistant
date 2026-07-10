# Enterprise AI Document Assistant

A focused React + ASP.NET Core application for connecting the core building blocks of modern AI applications: assistant UI, prompt orchestration, AI Gateway, RAG, vector search, Tool Calling, MCP, simple workflow orchestration, and Microsoft Graph integration.

V1 is intentionally small: one end-to-end document assistant flow, implemented in clear steps.

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
        gateway --> rag[Document RAG]
        workflows --> planner[Simple Agent Planner]
    end

    subgraph ai[AI + Data]
        rag --> vector[Vector Store]
        rag --> files[Document Storage]
        gateway --> models[OpenAI / Azure OpenAI]
        tools --> graph[Microsoft Graph]
        tools --> mcp[MCP Tools]
    end
```

---

## V1 Flow

```mermaid
sequenceDiagram
    participant U as User
    participant UI as React Workspace
    participant API as ASP.NET Core API
    participant AI as AI Gateway
    participant RAG as Document RAG
    participant Tools as Tool Gateway

    U->>UI: Ask about a document
    UI->>API: POST /api/chat
    API->>AI: Build prompt + route request
    AI->>RAG: Retrieve relevant chunks
    RAG-->>AI: Context + citations
    AI->>Tools: Optional controlled tool call
    Tools-->>AI: Tool result
    AI-->>API: Answer with citations
    API-->>UI: Stream response
```

---

## V1 Modules

| Module | Purpose | First Scope |
|---|---|---|
| React Workspace | User-facing work area | Document list, document detail, right-side Assistant, citations, tool results |
| ASP.NET Core API | Backend boundary | `/api/chat`, `/api/documents`, `/api/tools`, `/api/workflows` |
| AI Gateway | Model access layer | Chat calls, embedding calls, model/token/latency logging |
| Document RAG | Source-grounded answers | Upload, parse, chunk, embed, vector search, citations |
| Tool Gateway and Skills | Controlled actions | `SearchDocumentsTool`, `GetDocumentMetadataTool`, `SummarySkill`, `RiskAnalysisSkill` |
| MCP / Workflow / A2A | Extension path | MCP `search_documents`, simple planner, one workflow, optional two-agent flow |

---

## Current Status

- [x] React Workspace skeleton
- [ ] ASP.NET Core API skeleton
- [ ] Chat endpoint
- [ ] AI Gateway
- [ ] Document RAG
- [ ] Tool Gateway and Skills
- [ ] MCP and workflow extension

---

## Tech Stack

| Area | Stack |
|---|---|
| Frontend | React, TypeScript, Vite, Tailwind CSS |
| Backend | ASP.NET Core Web API |
| AI | OpenAI / Azure OpenAI, Semantic Kernel or Microsoft.Extensions.AI friendly design |
| Retrieval | Embeddings, vector store, source citations |
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

---

## Documentation

- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)
- [Chinese README](README.zh-CN.md)
