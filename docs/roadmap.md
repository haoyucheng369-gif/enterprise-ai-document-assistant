# Product Roadmap

This roadmap describes the planned evolution of Enterprise AI Document Assistant as a focused React + ASP.NET Core AI application. The first version should connect the core AI application concepts end to end without turning the project into a broad enterprise platform too early.

---

## V1 Goal

V1 should prove one complete path:

```text
React chat
   ↓
ASP.NET Core API
   ↓
Prompt orchestration
   ↓
AI Gateway
   ↓
Document ingestion and vector search
   ↓
RAG answer with citations
   ↓
Tool Calling
   ↓
Small Microsoft Graph / MCP / planner / workflow extension
```

The purpose of V1 is breadth across the main AI application concepts, not deep implementation of every enterprise feature.

---

## V1 Module Plan

V1 is implemented as six modules:

- React Workspace: document list, document detail, right-side AI Assistant, citation panel, tool result panel
- ASP.NET Core API: `/api/chat`, `/api/documents`, `/api/tools`, `/api/workflows`
- AI Gateway: chat calls, embedding calls, model/token/latency logging, simple provider configuration
- Document RAG: upload, parse text, chunk, embed, vector search, answer with citations
- Tool Gateway and Skills: `SearchDocumentsTool`, `GetDocumentMetadataTool`, `CreateEmailDraftTool` or `GetHealthStatusTool`, `SummarySkill`, `RiskAnalysisSkill`
- MCP, Workflow, and A2A Extension: MCP `search_documents`, document summary to risk analysis to email draft workflow, optional `DocumentAgent` and `EmailAgent`

---

## Phase 1 - Full-Stack Foundation

Objective: establish the React + ASP.NET Core foundation and make the assistant usable end to end.

Scope:

- React frontend foundation
- ASP.NET Core Web API foundation
- Conversation API
- Basic chat completion endpoint
- Streaming response support
- Configuration structure
- Initial local development setup

Expected outcome:

- The frontend can communicate with the backend.
- The user can send a message and receive an AI response.
- The project has a clean structure for the next AI capabilities.

---

## Phase 2 - Prompt Orchestration and AI Gateway

Objective: introduce a professional model access layer and reusable prompt structure.

Scope:

- AI Gateway abstraction
- OpenAI / Azure OpenAI provider configuration
- Retry and timeout handling
- Basic request logging
- Prompt templates
- Prompt variables
- Structured output
- AI output validation
- Lightweight guardrails
- Basic conversation memory
- Microsoft.Extensions.AI or Semantic Kernel friendly integration points

Expected outcome:

- Model calls are not scattered across business code.
- Prompt behavior is reusable and easier to test.
- The backend can evolve toward multiple model providers without changing the frontend.

---

## Phase 3 - Document Intelligence and RAG

Objective: enable source-grounded document question answering.

Scope:

- Document upload endpoint
- PDF / Word / text extraction
- Document metadata capture
- Chunking strategy
- Embedding generation through the AI Gateway
- Embedding lifecycle for document updates
- Vector store integration
- Semantic retrieval
- RAG-based question answering
- Source citation support

Expected outcome:

- A user can upload a document and ask questions about it.
- Answers are grounded in retrieved document sections.
- The UI can show sources for generated answers.

---

## Phase 4 - Tool Gateway and Light Enterprise Integration

Objective: let the assistant call controlled backend capabilities.

Scope:

- Tool Gateway
- Tool registration
- Tool argument validation
- Tool execution logging
- Document lookup tool
- Health check tool
- One SQL or REST data lookup tool
- One Microsoft Graph integration for an Outlook or Calendar scenario

Expected outcome:

- The assistant can use backend tools safely.
- Enterprise data and actions remain behind server-side APIs.
- Tool results can be included in assistant responses.

---

## Phase 5 - MCP Interface and Simple Workflow Orchestration

Objective: expose selected capabilities to external AI clients and coordinate simple multi-step AI flows.

Scope:

- MCP Server
- MCP tools backed by application services
- Reusable AI skills
- Simple Agent Planner
- One document summary or risk analysis workflow
- Optional email draft step
- Optional Agent-to-Agent collaboration as a small extension
- Basic workflow state persistence

Expected outcome:

- External MCP-compatible clients can call selected project capabilities.
- The assistant can coordinate document analysis, retrieval, tool execution, and business output generation in one workflow.

---

## Later Hardening

These items are important, but they can be added after the main application flow is working:

- Authentication and authorization
- Authorization-aware RAG retrieval
- Rate limiting
- Prompt injection mitigation
- Observability
- Health checks
- Docker-based deployment
- CI foundation
