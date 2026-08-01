# Architecture Overview

Enterprise AI Document Assistant is structured as a focused React + ASP.NET Core application connecting assistant UX, prompt orchestration, structured output, safety classification, AI Gateway, document intelligence, RAG, Tool Calling, an MCP-style surface, controlled planning, workflow orchestration, and typed agent handoff.

The architecture keeps V1 focused on one end-to-end assistant flow rather than a broad platform.

---

## High-Level Architecture

```text
                              Enterprise AI Document Assistant

┌─────────────────────────────────────────────────────────────────────────────┐
│                                React Frontend                                │
│                                                                             │
│       Document List  │  Preview and Insights  │  AI Assistant             │
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
│ Prompt Orchestration │ RAG │ Tool Calling │ Skills │ Planner │ Agents      │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
┌─────────────────────────┐ ┌──────────────────┐ ┌──────────────────────────┐
│   Document Pipeline      │ │ Enterprise Tools  │ │       AI Models           │
│                          │ │                  │ │                          │
│ Upload → Parse → Chunk   │ │ Graph / REST / DB │ │ Chat / Embedding Models   │
│ Embed → Retrieve         │ │ Health / MCP      │ │ Mock / OpenAI / Azure     │
└─────────────────────────┘ └──────────────────┘ └──────────────────────────┘
                    │                 │                 │
                    ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                                Persistence                                   │
│                                                                             │
│       MongoDB Documents / Conversations  │  Qdrant  │  In-memory Audit   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Main Components

### V1 Boundary

The first version should prove the whole path with minimal depth:

- One assistant UI
- One conversation API
- Shared and task-specific prompt templates
- Structured output, response validation, safety classification, and lightweight guardrails
- Basic conversation memory
- A small Tool Gateway
- One or two simple tools
- A minimal MCP-style HTTP surface for existing tools
- Prompt and tool harnesses
- Five reusable skills
- AI intent classification with rule fallback and a controlled planner
- In-memory structured audit and AI execution records
- One document ingestion path
- One RAG path with replaceable in-memory or Qdrant vector storage
- One simple workflow with a typed agent handoff
- A minimal Microsoft Graph adapter boundary

External authentication, persistent telemetry, source-file storage, advanced retrieval, and broad admin features remain later hardening items.

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
   - Safety classifier with rule-based and optional AI-backed routing
   - Simple guardrails
   - AI Gateway

4. Tool Gateway and Skills
   - `GetHealthStatusTool`
   - `GetDocumentMetadataTool`
   - `SummarySkill`
   - `RiskAnalysisSkill`
   - `EmailDraftSkill`
   - `ClassificationSkill`
   - `ResumeReviewSkill`
   - Conversation memory

5. Document RAG
   - Upload
   - Parse text
   - Chunk
   - Embed
   - Vector search
   - Answer with citations

6. MCP, Harness, Workflow, and Integration Extension
   - MCP-style interface exposing existing tools
   - Prompt and tool harnesses
   - Workflow: summarize document, identify risks, generate email draft
   - Microsoft Graph adapter boundary with mock email draft output
   - Typed A2A path: `DocumentAgent` handoff to `EmailAgent`

### React Frontend

The frontend provides the user-facing assistant and document experience.

Responsibilities:

- Chat-based interaction
- Chunked rendering from a completed structured response
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
- Internal AI Gateway provider routing
- Tool Gateway endpoints
- Integration endpoints
- MCP-style entry points

### AI Gateway

The AI Gateway is the boundary between application code and model providers.

Responsibilities:

- Model provider abstraction
- Chat and embedding request routing
- OpenAI / Azure OpenAI configuration
- Timeout and cancellation handling; automatic retry remains deferred
- Request logging
- Model selection

The backend should be compatible with Microsoft-friendly AI abstractions such as Semantic Kernel and Microsoft.Extensions.AI when they fit the implementation.

Current implementation:

- `IAiGateway` is the application-facing model boundary
- `MockAiGateway` returns structured assistant messages without calling an external provider
- `OpenAiGateway` supports configurable OpenAI and Azure OpenAI chat completions
- Gateway responses include provider, model, latency, and token estimates
- Gateway calls are recorded in the audit trail

Classification and the implemented skills already use the selected provider. Dedicated structured extraction remains deferred.

### Prompt Orchestration

Prompt orchestration manages repeatable AI behavior instead of scattering prompt strings across controllers or UI code.

Responsibilities:

- Common enterprise system prompt defaults
- Prompt templates
- Task-specific prompt instructions
- Runtime variables
- Output rules
- Structured output schemas
- AI output validation
- Safety classification before planner or model execution, with deterministic fallback when model classification is unavailable
- Guardrails
- Reusable AI skills

Current code keeps the shared assistant behavior in `EnterpriseAssistantPromptDefaults`, the chat template in `DocumentAssistantPrompt`, and skill-specific AI templates in `DocumentSkillPromptTemplates`.

### Skills

Skills package a focused AI capability behind a stable input and output contract.

Current skills:

- `SummarySkill`: summarizes a selected document through `POST /api/skills/summary`
- `RiskAnalysisSkill`: extracts risk items through `POST /api/skills/risk-analysis`
- `EmailDraftSkill`: composes summary, risk analysis, document metadata tool output, and AI Gateway generation through `POST /api/skills/email-draft`
- `ClassificationSkill`: classifies document type, priority, and risk level
- `ResumeReviewSkill`: generates a Markdown resume review brief through `POST /api/skills/resume-review`

Each document skill keeps a deterministic Mock path for local testing and an AI Gateway path for OpenAI or Azure OpenAI execution.

Deferred skills can extend the same prompt/template/output contract pattern for structured extraction or document generation exports.

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

Current endpoint:

- `POST /api/documents/upload`
- `GET /api/documents/uploads`

Current implementation extracts lightweight preview text for `.txt`, `.md`, `.pdf`, and `.docx`, stores parsed sections in MongoDB, and indexes chunks for the first RAG path.

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
- No-answer behavior when reliable context is missing
- Later hybrid search and semantic ranking hooks

Current implementation:

- `IEmbeddingGateway` converts document chunks and user questions into vectors.
- `RoutingEmbeddingGateway` uses deterministic local vectors for Mock and real OpenAI/Azure OpenAI embedding calls when a real provider is selected.
- `IVectorStore` keeps vector search replaceable.
- `InMemoryVectorStore` keeps a process-local comparison implementation.
- `QdrantVectorStore` persists vectors and chunk payloads in provider-specific Qdrant collections.
- `RagService` indexes uploaded chunks, lazily rebuilds missing provider-specific indexes, retrieves top chunks, and returns citations from the matched chunks.

Configuration boundary:

- `Rag:VectorStore` selects `InMemory` or `Qdrant` through dependency injection.
- MongoDB Atlas Vector Search or Azure AI Search can later replace Qdrant without changing chat orchestration.

### Tool Gateway

The Tool Gateway exposes controlled backend capabilities to the AI layer.

For explicit tool requests, V1 uses a bounded single-turn loop:

```text
User request
  -> model selects one registered read-only tool
  -> Tool Gateway validates and executes it
  -> tool result returns to the model
  -> structured assistant response
```

The loop does not recurse and does not permit write tools, keeping execution predictable.

Current tools:

- Document metadata lookup
- Health status lookup

Possible later tools:

- Document search
- SQL-backed data lookup
- A small Microsoft Graph adapter operation

Responsibilities:

- Tool registration
- Argument validation
- Controlled execution
- Execution logging
- Result formatting for AI responses

### MCP Interface

The MCP interface exposes selected Tool Gateway capabilities through simplified MCP-style discovery and call endpoints. It demonstrates `list + call`, but it is not a complete MCP SDK/JSON-RPC transport implementation.

Current endpoints:

- `GET /api/mcp/tools/list`
- `POST /api/mcp/tools/call`

Responsibilities:

- Convert internal tool definitions into MCP-style tool descriptors
- Convert MCP tool call requests into `ToolExecutionRequest`
- Reuse the existing Tool Gateway executor instead of duplicating business logic
- Keep external tool exposure separate from internal tool implementation

### Conversation Memory

Conversation memory keeps recent context available for follow-up questions, while MongoDB restores validated turns after an API or browser restart.

Responsibilities:

- Select recent user and assistant turns
- Format recent context for prompt variables
- Keep memory short and request-scoped at first
- Support later planner and workflow decisions

Current implementation:

- `ConversationMemoryBuilder` reads recent turns from `ChatRequest.History`
- `DocumentAssistantPromptOrchestrator` injects `conversation_memory` into the rendered prompt
- `MongoConversationRepository` stores one complete user/assistant turn per MongoDB record
- `WorkspaceDataProvider` restores the latest persisted turns for the React workspace
- The harness verifies that recent context is included before model integration

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

### Intent Classification and Agent Planner

Request routing has three explicit responsibilities:

```text
Intent Classifier -> Agent Planner -> Planned Capability Executor
```

- `RoutingIntentClassifier` classifies the request with AI first and deterministic rules as fallback.
- `AgentPlanner` maps the intent to one controlled route and a known set of steps.
- `PlannedCapabilityExecutor` executes the selected Skill, Workflow, Tool Calling path, or default RAG chat.

This request intent classification is separate from `ClassificationSkill`, which classifies the selected document as a business task.

Example plans:

- Answer a document question with RAG
- Summarize a selected document
- Analyze risks in a selected document
- Generate an email draft after document analysis
- Call a backend tool and explain the result

Responsibilities:

- Plan selection
- Intent-to-route mapping
- Known step and capability description

Current endpoint:

- `POST /api/planner/plan`

### Simple Workflow

The first workflow coordinates two focused agents in a fixed sequence instead of introducing a full workflow engine.

Current sequence:

- `DocumentAgent` runs `SummarySkill` and `RiskAnalysisSkill`
- `DocumentAgentHandoff` carries typed analysis output
- `EmailAgent` consumes the handoff through `EmailDraftSkill`

Current endpoint:

- `POST /api/workflows/document-review`

### Agent Orchestration And A2A

Agent orchestration is intentionally limited to one controlled handoff.

Current shape:

- Existing `AgentPlanner`: selects the known workflow path
- `DocumentAgent`: summarizes documents and identifies risks
- `EmailAgent`: drafts follow-up email content
- `DocumentAgentHandoff`: passes structured document analysis between the agents

### Persistence

Persistence stores application state without making any single database the center of the architecture.

Storage boundaries:

- MongoDB: document records, ACL metadata, parsed sections, and conversations
- Vector store: embeddings and semantic retrieval indexes
- In-memory stores: audit, tool execution, and workflow execution records
- Deferred file or object storage: uploaded source documents

Current implementation:

- MongoDB stores uploaded document metadata, parsed sections, and complete validated conversation turns
- Document records carry an owner and allowed-reader ACL that MongoDB applies before RAG, Skill, or Tool access
- Qdrant stores persistent embedding vectors used by semantic retrieval
- In-memory audit and vector implementations remain available behind interfaces where useful
- Uploaded source files, workflow records, and audit events are not persisted yet
- Storage remains behind repository interfaces so business and AI flows do not depend on database drivers

---

## AI Execution Flow

```text
User message
   ↓
HTTP validation, ACL, and rate limiting
   ↓
Input guardrails
   ↓
Intent Classifier and Agent Planner
   ↓
Planned Capability Executor
   ↓
Skill / Workflow / Tool Calling / RAG
   ↓
Prompt Orchestration and AI Gateway when the selected capability needs a model
   ↓
Structured output validation
   ↓
Conversation persistence for the primary chat endpoint
   ↓
Response formatting and chunked rendering to React UI
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
Embedding through Embedding Gateway
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

## Audit Trail

The first audit trail records key AI application decisions and executions in memory.

Current endpoint:

- `GET /api/audit/events`
- `GET /api/audit/ai-executions`

Tracked categories:

- `chat`
- `safety`
- `planner`
- `tool`
- `skill`
- `integration`
- `workflow`
- `ai_gateway`

The current implementation is intentionally replaceable. A later infrastructure step can swap `InMemoryAuditLogger` for structured logging, database storage, or OpenTelemetry without changing the calling code.

---

## Integration Strategy

Enterprise integrations are isolated behind adapters and tools.

Examples:

- Microsoft Graph Adapter
- REST API Adapter
- SQL Data Adapter
- Health Check Adapter
- MCP Tool Adapter

Current implementation:

- `IMicrosoftGraphGateway` is the enterprise integration boundary
- `MockMicrosoftGraphGateway` returns an Outlook-style draft response without OAuth
- `POST /api/integrations/graph/email-draft` exposes the first Graph integration shape

This allows the assistant to use enterprise capabilities without coupling prompts or UI components directly to external SDKs.

---

## Design Constraints

- Keep the first implementation small enough to build incrementally.
- Prefer one working vertical slice over many shallow modules.
- The frontend must not call AI providers or enterprise systems directly.
- Model access must go through the AI Gateway.
- AI-invoked backend actions should go through the Tool Gateway.
- RAG answers must include traceable citations.
- Prompt templates should be versionable and testable.
- Structured outputs should be validated before they are used by the UI or workflows.
- Guardrails should start with structured safety classification, simple rules, and deterministic fallback, then evolve toward stronger policies.
- The MCP-style surface must reuse registered Tool Gateway definitions and execution.
- The Agent Planner should choose from known paths instead of performing open-ended autonomous planning.
- Persistence should be replaceable where possible.
- Agent orchestration should remain a controlled typed handoff rather than an open-ended autonomous loop.
