# Product Roadmap

This roadmap keeps V1 focused: connect the common AI application concepts in a React + ASP.NET Core assistant without turning the project into a broad enterprise platform too early.

---

## V1 Goal

V1 should prove one complete path:

```text
React chat
  -> ASP.NET Core API
  -> Prompt orchestration
  -> Structured output validation
  -> Simple guardrails
  -> Tool Gateway
  -> First tools
  -> MCP Server
  -> Prompt and Tool Harness
  -> Reusable skills
  -> Conversation Memory
  -> Simple planner
  -> Audit logging
  -> Document ingestion
  -> workflow extension
  -> light enterprise integration
  -> agent handoff extension
  -> RAG with citations
```

The implementation order intentionally brings Tool, MCP, Skill, and Harness concepts earlier because they are common, concrete, and easier to understand before full RAG.

---

## V1 Module Plan

V1 is implemented as six modules:

- React Workspace: document list, document detail, right-side AI Assistant, citation panel, tool result panel
- ASP.NET Core API: `/api/chat`, `/api/documents`, `/api/tools`, `/api/workflows`
- Prompt and AI Layer: prompt orchestration, conversation memory, structured output, validation, guardrails, AI Gateway
- Tool Gateway and Skills: `GetHealthStatusTool`, `GetDocumentMetadataTool`, `SummarySkill`, `RiskAnalysisSkill`, `EmailDraftSkill`
- Document RAG: upload, parse text, chunk, embed, vector search, answer with citations
- MCP, Harness, Workflow, and Agent Orchestration Extension: MCP `search_documents`, prompt/tool harnesses, document summary to risk analysis to email draft workflow, coordinator-to-agent orchestration, optional `DocumentAgent` to `EmailAgent` handoff

---

## Phase 1 - Full-Stack Foundation

Objective: establish the React + ASP.NET Core foundation and make the assistant usable end to end.

Scope:

- React workspace
- ASP.NET Core Web API
- Backend-driven workspace data
- Chat endpoint
- Streaming response support
- Local development setup

Expected outcome:

- The frontend can communicate with the backend.
- The user can send a message and receive a streamed assistant response.

---

## Phase 2 - Prompt Control

Objective: make AI responses easier to control before adding tools or model providers.

Scope:

- Prompt templates
- Prompt variables
- Prompt orchestration
- Structured output contract
- Structured output validation
- Simple guardrails
- Basic conversation memory

Expected outcome:

- Prompt behavior is reusable and testable.
- Assistant responses have a predictable shape.
- Unsafe or unsupported requests can be handled consistently.

---

## Phase 3 - Tool Gateway and First Tools

Objective: let the assistant call controlled backend capabilities before full document RAG.

Scope:

- Tool Gateway
- Tool registration
- Tool argument validation
- Tool execution logging
- `GetHealthStatusTool`
- `GetDocumentMetadataTool`
- Tool result formatting for assistant responses

Expected outcome:

- The assistant can use simple backend tools safely.
- Tool results can be shown in the UI and reused by later planner or workflow steps.

---

## Phase 4 - MCP Server and Harnesses

Objective: expose selected tools through MCP and add lightweight verification for prompt and tool behavior.

Scope:

- Minimal MCP Server
- MCP `get_health_status` and `get_document_metadata`
- MCP request/response contract
- Prompt harness with a few fixed test cases
- Tool harness for argument validation and result shape checks
- `GET /api/harness` report for prompt, guardrail, structured output, and tool checks
- Basic audit log shape for tool and MCP calls

Expected outcome:

- An MCP-style client can discover and call selected tools.
- Prompt and tool behavior can be checked with repeatable inputs.
- Tool and MCP executions have a traceable record shape.

---

## Phase 5 - Reusable Skills, Memory, and Simple Planner

Objective: introduce reusable AI capabilities, short-term conversation memory, and a deterministic planner.

Scope:

- `SummarySkill`
- `RiskAnalysisSkill`
- `EmailDraftSkill`
- `POST /api/skills/summary`
- `POST /api/skills/risk-analysis`
- `POST /api/skills/email-draft`
- Conversation memory from recent chat history
- Memory injection into prompt variables
- Simple Agent Planner
- Planner routes to known paths: answer, summarize, risk analysis, tool lookup

Expected outcome:

- Common AI behaviors are modular.
- The assistant can use recent context when the user asks follow-up questions.
- The assistant can choose a small number of predictable paths without open-ended autonomy.

---

## Phase 6 - AI Gateway

Objective: introduce a professional model access layer.

Scope:

- AI Gateway abstraction
- OpenAI / Azure OpenAI provider configuration
- Chat model calls
- Retry and timeout handling
- Basic request logging
- Model, token, and latency metadata
- Microsoft.Extensions.AI or Semantic Kernel friendly integration points

Expected outcome:

- Model calls are not scattered across business code.
- The backend can evolve toward multiple model providers without changing the frontend.

Current V1 status:

- AI Gateway abstraction is in place with `MockAiGateway`
- Real OpenAI or Azure OpenAI providers remain a later provider implementation step

---

## Phase 7 - Document Ingestion and Parsing

Objective: establish the document ingestion path before the full RAG slice.

Scope:

- Document upload endpoint
- Text extraction
- Document metadata capture
- Chunking strategy

Expected outcome:

- A user can upload a supported document.
- The backend can validate the file and return document metadata.
- Later retrieval steps have a clear entry point.

---

## Phase 8 - Workflow and Light Enterprise Integration

Objective: coordinate simple multi-step flows and connect one light enterprise integration before the full RAG slice.

Scope:

- Simple workflow: summarize document, identify risks, generate email draft
- One Microsoft Graph integration for an Outlook or Calendar scenario
- Optional Agent-to-Agent handoff: `DocumentAgent` to `EmailAgent`
- Basic workflow state persistence

Expected outcome:

- The assistant can coordinate document analysis, tool execution, and business output generation in one small workflow.

---

## Phase 9 - Document RAG

Objective: enable source-grounded document question answering after ingestion and workflow concepts are in place.

Scope:

- Embedding generation through the AI Gateway
- Vector store integration
- Semantic retrieval
- RAG-based question answering
- Source citation support

Expected outcome:

- A user can upload a document and ask questions about it.
- Answers are grounded in retrieved document sections.
- The UI can show sources for generated answers.

---

## Later Hardening

These items are important, but they can be added after the main application flow is working:

- Authentication and authorization
- Authorization-aware RAG retrieval
- Rate limiting
- Prompt injection mitigation
- Observability
- Expanded harnesses for prompts, tools, skills, and workflows
- Docker-based deployment
- CI foundation
