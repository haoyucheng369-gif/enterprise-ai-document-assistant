# Architecture Overview

Enterprise AI Document Assistant is structured as a focused React + ASP.NET Core application for connecting the core concepts of modern AI software: assistant UX, prompt orchestration, structured output, AI Gateway, document intelligence, RAG, Tool Calling, MCP, simple Agent Planner, and workflow orchestration.

The architecture keeps the first version small. It supports enterprise-style patterns, but the initial implementation should be one narrow end-to-end assistant flow rather than a broad platform.

---

## High-Level Architecture

```text
                              Enterprise AI Document Assistant

┌─────────────────────────────────────────────────────────────────────────────┐
│                                React Frontend                                │
│                                                                             │
│   AI Assistant  │  Document Center  │  Workflow View  │  Integration View   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ASP.NET Core Web API                              │
│                                                                             │
│ Conversations │ Documents │ AI Gateway │ Tool Gateway │ Integrations │ MCP  │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            AI Application Layer                              │
│                                                                             │
│ Prompt Orchestration │ RAG │ Tool Calling │ Skills │ Planner │ Workflows   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
┌─────────────────────────┐ ┌──────────────────┐ ┌──────────────────────────┐
│   Document Pipeline      │ │ Enterprise Tools  │ │       AI Models           │
│                          │ │                  │ │                          │
│ Upload → Parse → Chunk   │ │ Graph / REST / DB │ │ Chat / Embedding Models   │
│ Embed → Retrieve         │ │ Health / MCP      │ │ OpenAI / Azure OpenAI     │
└─────────────────────────┘ └──────────────────┘ └──────────────────────────┘
                    │                 │                 │
                    ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                                Persistence                                   │
│                                                                             │
│   SQL Server / PostgreSQL  │  Vector Store  │  File Storage  │  AI Records   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Main Components

### V1 Boundary

The first version should prove the whole path with minimal depth:

- One assistant UI
- One conversation API
- One or two prompt templates
- Structured output, response validation, and lightweight guardrails
- Basic conversation memory
- A small Tool Gateway
- One or two simple tools
- A minimal MCP Server for existing tools
- Prompt and tool harnesses
- Three reusable skills
- One deterministic planner
- Basic audit log shape
- One document ingestion path
- One vector search path
- One simple workflow
- A minimal Microsoft Graph integration

Advanced security, observability, multi-tenant authorization, queue monitoring, and broad admin features are later hardening items.

### V1 Module Map

The first implementation is grouped into six modules:

1. React Workspace
   - Document list
   - Document detail view
   - Right-side AI Assistant
   - Citation panel
   - Tool result panel

2. ASP.NET Core API
   - `/api/chat`
   - `/api/documents`
   - `/api/tools`
   - `/api/workflows`

3. Prompt and AI Layer
   - Prompt orchestration
   - Structured output
   - Validation
   - Simple guardrails
   - AI Gateway

4. Tool Gateway and Skills
   - `SearchDocumentsTool`
   - `GetDocumentMetadataTool`
   - `CreateEmailDraftTool` or `GetHealthStatusTool`
   - `SummarySkill`
   - `RiskAnalysisSkill`
   - `EmailDraftSkill`
   - Conversation memory

5. Document RAG
   - Upload
   - Parse text
   - Chunk
   - Embed
   - Vector search
   - Answer with citations

6. MCP, Harness, Workflow, and Agent Orchestration Extension
   - MCP Server exposing existing tools first
   - Prompt and tool harnesses
   - Workflow: summarize document, identify risks, generate email draft
   - Coordinator-to-agent orchestration
   - Optional A2A path: `DocumentAgent` handoff to `EmailAgent`

### React Frontend

The frontend provides the user-facing assistant and document experience.

Responsibilities:

- Chat-based interaction
- Streaming response rendering
- Basic conversation memory
- Document upload
- Source citation display
- Tool execution result display
- Basic workflow status display

### ASP.NET Core Web API

The backend exposes application APIs and keeps model calls, document processing, tools, and integrations behind server-side boundaries.

Responsibilities:

- Conversation endpoints
- Document endpoints
- AI Gateway endpoints
- Tool Gateway endpoints
- Integration endpoints
- MCP-compatible entry points

### AI Gateway

The AI Gateway is the boundary between application code and model providers.

Responsibilities:

- Model provider abstraction
- Chat and embedding request routing
- OpenAI / Azure OpenAI configuration
- Retry and timeout handling
- Request logging
- Model selection

The backend should be compatible with Microsoft-friendly AI abstractions such as Semantic Kernel and Microsoft.Extensions.AI when they fit the implementation.

### Prompt Orchestration

Prompt orchestration manages repeatable AI behavior instead of scattering prompt strings across controllers or UI code.

Responsibilities:

- Prompt templates
- Runtime variables
- Structured output schemas
- AI output validation
- Guardrails
- Reusable AI skills

### Skills

Skills package a focused AI capability behind a stable input and output contract.

Current skill:

- `SummarySkill`: summarizes a selected document through `POST /api/skills/summary`
- `RiskAnalysisSkill`: extracts risk items through `POST /api/skills/risk-analysis`
- `EmailDraftSkill`: drafts a follow-up email through `POST /api/skills/email-draft`

### Document Intelligence

Document Intelligence converts uploaded files into usable text and metadata.

Responsibilities:

- File upload handling
- Text extraction
- Metadata extraction
- Chunking
- Embedding lifecycle support for document updates
- Document summary preparation
- Risk and obligation extraction support

### RAG

RAG connects document retrieval with model responses.

Responsibilities:

- Embedding generation
- Vector indexing
- Embedding lifecycle management
- Semantic search
- Context retrieval
- Citation tracking
- Grounded answer generation

### Tool Gateway

The Tool Gateway exposes controlled backend capabilities to the AI layer.

Example tools:

- Document search
- Document metadata lookup
- Health check lookup
- SQL-backed data lookup
- A small Microsoft Graph operation
- MCP-compatible tools

Responsibilities:

- Tool registration
- Argument validation
- Controlled execution
- Execution logging
- Result formatting for AI responses

### MCP Interface

The MCP interface exposes selected Tool Gateway capabilities through MCP-style discovery and call endpoints.

Current endpoints:

- `GET /api/mcp/tools/list`
- `POST /api/mcp/tools/call`

Responsibilities:

- Convert internal tool definitions into MCP-style tool descriptors
- Convert MCP tool call requests into `ToolExecutionRequest`
- Reuse the existing Tool Gateway executor instead of duplicating business logic
- Keep external tool exposure separate from internal tool implementation

### Conversation Memory

Conversation memory keeps recent context available for follow-up questions without requiring database persistence in the first version.

Responsibilities:

- Select recent user and assistant turns
- Format recent context for prompt variables
- Keep memory short and request-scoped at first
- Support later planner and workflow decisions

### Harnesses

Harnesses provide repeatable checks for AI-facing capabilities without requiring a large test platform.

Examples:

- Prompt harness: run fixed inputs through prompt orchestration and validate structured output
- Tool harness: run tools with valid and invalid arguments and validate result shapes
- Skill harness: run summary, risk analysis, or email draft skills and validate required fields

Responsibilities:

- Fixed test cases
- Expected output shape checks
- Guardrail checks
- Simple execution reports

Current endpoint:

- `GET /api/harness`

### Simple Agent Planner

The first planner should remain deterministic and small. It should choose from a few known paths instead of attempting open-ended autonomous planning.

Example plans:

- Answer a document question with RAG
- Summarize a selected document
- Analyze risks in a selected document
- Generate an email draft after document analysis
- Call a backend tool and explain the result

Responsibilities:

- Intent classification
- Plan selection
- Skill and tool sequencing
- Basic plan result formatting

### Agent Orchestration And A2A

Agent orchestration remains a later extension after workflow is stable.

Planned shape:

- `CoordinatorAgent`: selects a known path
- `DocumentAgent`: summarizes documents and identifies risks
- `EmailAgent`: drafts follow-up email content
- A2A handoff: pass structured document analysis from `DocumentAgent` to `EmailAgent`

### Persistence

Persistence stores application state without making any single database the center of the architecture.

Storage responsibilities:

- Relational database: users, documents, conversations, tool executions, workflow records
- Vector store: embeddings and semantic retrieval indexes
- File or object storage: uploaded source documents
- Optional document database or JSON columns: conversation memory, flexible AI records, and workflow state

---

## AI Execution Flow

```text
User message
   ↓
Conversation API
   ↓
Prompt Orchestration
   ↓
Structured output and guardrails
   ↓
Tool Gateway and/or Skill execution
   ↓
Conversation Memory
   ↓
Simple Agent Planner
   ↓
AI Gateway and/or RAG retrieval
   ↓
Response formatting with citations and tool results
   ↓
Streaming response to React UI
```

---

## Document Question Flow

```text
Upload document
   ↓
Document API
   ↓
Text extraction and metadata capture
   ↓
Chunking
   ↓
Embedding through AI Gateway
   ↓
Vector store indexing
   ↓
Question
   ↓
Semantic retrieval
   ↓
Grounded answer with citations
```

---

## Integration Strategy

Enterprise integrations are isolated behind adapters and tools.

Examples:

- Microsoft Graph Adapter
- REST API Adapter
- SQL Data Adapter
- Health Check Adapter
- MCP Tool Adapter

This allows the assistant to use enterprise capabilities without coupling prompts or UI components directly to external SDKs.

---

## Design Constraints

- Keep the first implementation small enough to build incrementally.
- Prefer one working vertical slice over many shallow modules.
- The frontend must not call AI providers or enterprise systems directly.
- Model access must go through the AI Gateway.
- Enterprise actions must go through the Tool Gateway.
- RAG answers must include traceable citations.
- Prompt templates should be versionable and testable.
- Structured outputs should be validated before they are used by the UI or workflows.
- Guardrails should be simple rules at first, such as requiring citations for document answers.
- MCP can be introduced once at least one backend tool exists.
- The Agent Planner should choose from known paths instead of performing open-ended autonomous planning.
- Persistence should be replaceable where possible.
- Agent orchestration and A2A should remain optional extension points until the workflow is stable.
