# Enterprise AI Document Assistant

**Enterprise AI Document Assistant** is a focused AI application that shows how modern Large Language Models can be integrated into enterprise-style software using React, ASP.NET Core, Retrieval-Augmented Generation, AI orchestration, tool calling, MCP, and Microsoft Graph.

The project focuses on a practical first version: build one end-to-end assistant flow, connect documents to AI responses, call a few backend tools, and keep the architecture clear enough to extend later.

---

## Product Positioning

Enterprise teams often work with contracts, technical notes, reports, procedures, emails, and internal knowledge spread across different systems.

This application starts with an AI assistant that can:

- Chat with users through a React interface
- Upload and process business documents
- Answer document questions with source citations
- Use prompt orchestration for repeatable AI tasks
- Call controlled backend tools through a Tool Gateway
- Integrate with Microsoft Graph for a small Outlook or Calendar scenario
- Expose a small MCP-compatible interface
- Coordinate a simple multi-step AI workflow

The goal is not to build a large enterprise platform in the first version. The goal is to connect the core concepts of modern AI applications in a clean React + ASP.NET Core architecture.

---

## V1 Scope

The first version should be a narrow vertical slice:

- A React chat interface
- An ASP.NET Core conversation endpoint
- Prompt orchestration for one or two reusable tasks
- Structured output, AI output validation, and lightweight guardrails
- AI Gateway for chat and embedding calls
- Document upload, chunking, embedding, and vector search
- Basic conversation memory
- RAG answer generation with source citations
- A small Tool Gateway with a few backend tools
- A minimal Microsoft Graph integration
- A minimal MCP Server and client path
- A simple Agent Planner that chooses one known workflow path
- A simple workflow that combines document analysis and tool execution

Everything else should remain lightweight until this path works end to end.

---

## V1 Modules

The first version is organized around six practical modules instead of a broad enterprise platform.

### 1. React Workspace

- Document list
- Document detail view
- Right-side AI Assistant
- Citation panel
- Tool result panel

### 2. ASP.NET Core API

- `/api/chat`
- `/api/documents`
- `/api/tools`
- `/api/workflows`

### 3. AI Gateway

- Unified chat model calls
- Unified embedding model calls
- Model, token, and latency logging
- Simple provider configuration without complex fallback logic

### 4. Document RAG

- Upload
- Parse text
- Chunk
- Embed
- Vector search
- Answer with citations

### 5. Tool Gateway and Skills

- `SearchDocumentsTool`
- `GetDocumentMetadataTool`
- `CreateEmailDraftTool` or `GetHealthStatusTool`
- `SummarySkill`
- `RiskAnalysisSkill`

### 6. MCP, Workflow, and A2A Extension

- MCP Server exposing `search_documents`
- Workflow: summarize document, identify risks, generate email draft
- Optional A2A path: `DocumentAgent` and `EmailAgent`

---

## Core Capabilities

### AI Assistant

- Conversation API
- Streaming responses
- Multi-turn context
- React-based assistant interface
- Source and tool result rendering
- Basic conversation memory

### Prompt Orchestration

- Prompt templates
- Runtime variables
- Structured output
- AI output validation
- Guardrails
- Reusable AI skills

### Document Intelligence

- Document upload
- Text extraction
- Metadata management
- Chunking
- Document summaries
- Risk and obligation extraction

### AI Gateway

- Model provider abstraction
- OpenAI / Azure OpenAI ready design
- Microsoft.Extensions.AI and Semantic Kernel friendly architecture
- Retry and timeout handling
- Request logging
- Model configuration

### RAG

- Embedding generation
- Embedding lifecycle for document updates
- Vector database integration
- Semantic search
- Context retrieval
- Source citation tracking
- Grounded document question answering

### Tool Gateway

- Backend tool registration
- Tool Calling execution
- Input validation
- Execution audit trail
- Enterprise API adapters

### Persistence

- Relational database for structured application entities
- Document metadata storage
- Conversation history
- Workflow state
- Optional MongoDB or JSON-based storage for flexible AI records

### Enterprise Integration

- Microsoft Graph for Outlook or Calendar scenarios
- REST API connector pattern
- SQL-backed data lookup pattern
- Health check tool

### MCP Interface

- MCP Server
- MCP tools backed by application services
- Small external AI client access path

### Workflow Orchestration

- Simple Agent Planner
- AI skill composition
- Simple multi-step document workflow
- Tool execution pipeline
- Optional Agent-to-Agent collaboration as an extension

---

## Reference Architecture

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

## Core Flows

### Document Question Answering

```text
Upload document
      ↓
Extract text and metadata
      ↓
Chunk and embed content
      ↓
Store vectors and document metadata
      ↓
Ask a question
      ↓
Retrieve relevant chunks
      ↓
Generate answer with citations
```

### Tool Calling

```text
User asks for an action or enterprise data
      ↓
Assistant selects a backend tool
      ↓
Tool Gateway validates arguments
      ↓
Backend executes the controlled operation
      ↓
Result is added to the AI response
```

### AI Workflow

```text
Select document
      ↓
Agent Planner chooses a simple plan
      ↓
Run summary, risk analysis, or email draft skill
      ↓
Retrieve supporting evidence
      ↓
Generate structured output
      ↓
Optionally create a follow-up email or task
```

---

## Technology Stack

### Frontend

- React
- TypeScript
- Vite
- AI assistant UI
- Document upload UI
- Citation and tool result panels

### Backend

- ASP.NET Core Web API
- Service-oriented application structure
- Conversation endpoints
- Document endpoints
- AI Gateway
- Tool Gateway
- Integration adapters

### AI Application Layer

- Chat completion
- Embeddings
- Prompt orchestration
- Structured output
- AI output validation
- Guardrails
- RAG
- Tool Calling
- Simple Agent Planner
- MCP-compatible tool exposure
- Workflow orchestration
- Optional A2A pattern

### Storage

- SQL Server or PostgreSQL
- Vector store
- File or object storage
- Optional MongoDB for flexible AI state

### Integrations

- Microsoft Graph
- REST APIs
- SQL-backed services
- Health check APIs
- MCP clients and servers

---

## Repository Structure

```text
enterprise-ai-document-assistant/
│
├── frontend/                  # React + TypeScript application
├── backend/                   # ASP.NET Core Web API
│   ├── src/
│   └── tests/
│
├── docs/
│   ├── architecture.md        # Architecture overview
│   ├── roadmap.md             # Product roadmap
│   └── decisions/             # Architecture Decision Records
│
├── infrastructure/
│   ├── docker/
│   └── scripts/
│
├── samples/
│   ├── documents/
│   └── requests/
│
├── README.md
└── README.zh-CN.md
```

---

## Roadmap

### Phase 1 - Full-Stack Foundation

- React frontend foundation
- ASP.NET Core Web API foundation
- Basic AI assistant endpoint
- Streaming conversation response
- Configuration structure

### Phase 2 - Prompt and AI Gateway

- Prompt orchestration
- Structured output
- AI output validation and guardrails
- AI Gateway abstraction
- Model provider configuration

### Phase 3 - Document Intelligence and RAG

- Document upload and parsing
- Chunking
- Embedding generation
- Embedding lifecycle
- Vector search
- Source-grounded document question answering

### Phase 4 - Tool Gateway and Light Enterprise Integration

- Tool Calling through backend services
- Microsoft Graph integration
- SQL / REST tool adapters
- Health check tool
- Tool execution audit

### Phase 5 - MCP and Simple Workflow Orchestration

- MCP Server
- MCP-backed application tools
- AI skills
- Simple Agent Planner
- One multi-step workflow
- Optional A2A collaboration

---

## Local Development

### Prerequisites

- Node.js LTS
- .NET SDK
- Docker Desktop
- Git

### Clone the repository

```bash
git clone https://github.com/haoyucheng369-gif/enterprise-ai-document-assistant.git
cd enterprise-ai-document-assistant
code .
```

The implementation will be built progressively inside `frontend/`, `backend/`, `docs/`, and `infrastructure/`.

---

## Design Principles

- Keep the scope practical and focused on one end-to-end AI application path.
- Use product-oriented names instead of temporary or classroom-style terminology.
- Keep the frontend, backend, AI orchestration, and integrations clearly separated.
- Keep model access behind an AI Gateway.
- Keep enterprise actions behind a Tool Gateway.
- Make document answers traceable through citations.
- Prefer Microsoft-friendly AI architecture such as Semantic Kernel and Microsoft.Extensions.AI when it fits the .NET backend.

---

## Status

Architecture and roadmap are initialized. Implementation will proceed through the roadmap phases.
