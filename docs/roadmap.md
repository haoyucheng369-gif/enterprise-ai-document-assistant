# Product Roadmap

This roadmap describes the planned evolution of Enterprise AI Document Assistant as a professional enterprise AI application.

---

## Phase 1 - Core Platform

Objective: establish the full-stack foundation.

Scope:

- React portal foundation
- ASP.NET Core Web API foundation
- Basic AI assistant endpoint
- Initial document upload endpoint
- Configuration structure
- Docker-ready development environment

Expected outcome:

- The frontend can communicate with the backend.
- The backend exposes a clean API foundation.
- The project has a stable structure for future capabilities.

---

## Phase 2 - Document Intelligence

Objective: enable AI-powered document understanding.

Scope:

- PDF / Word / text extraction
- Document chunking
- Embedding generation
- Vector indexing
- RAG-based question answering
- Source citation support

Expected outcome:

- A user can upload a document and ask questions about it.
- Answers are grounded in retrieved document sections.
- Sources are traceable.

---

## Phase 3 - AI Capabilities

Objective: add reusable AI capabilities on top of the document intelligence layer.

Scope:

- Prompt orchestration
- Document summary skill
- Risk analysis skill
- Email generation skill
- Structured output generation
- Conversation persistence

Expected outcome:

- The assistant can execute business-oriented AI tasks.
- Capabilities are reusable and not hard-coded into the UI.

---

## Phase 4 - Enterprise Integrations

Objective: connect the assistant to controlled enterprise systems.

Scope:

- Microsoft Graph integration
- SQL / REST tool connectors
- System health tool
- Queue status tool
- MCP Server for AI-compatible tool exposure

Expected outcome:

- The assistant can call backend tools safely.
- Enterprise data remains protected behind backend APIs.

---

## Phase 5 - Workflow Orchestration

Objective: support multi-step AI-assisted business workflows.

Scope:

- Workflow execution pipeline
- AI Skills composition
- Tool execution lifecycle
- Workflow state persistence
- Optional specialized agent collaboration

Expected outcome:

- The assistant can coordinate document analysis, tool execution, and business output generation in one workflow.

---

## Phase 6 - Enterprise Readiness

Objective: harden the platform for enterprise-grade expectations.

Scope:

- Authorization-aware retrieval
- Rate limiting
- Retry and timeout handling
- Prompt injection protection
- Observability
- Health checks
- Docker-based deployment
- CI foundation

Expected outcome:

- The platform follows professional engineering standards and is ready for more advanced implementation.
