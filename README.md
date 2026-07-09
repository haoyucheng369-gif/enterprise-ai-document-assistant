# Enterprise AI Document Assistant

**Enterprise AI Document Assistant** is an AI-powered enterprise application designed to help organizations analyze, search, understand, and act on business documents through Large Language Models.

The platform combines document intelligence, Retrieval-Augmented Generation, semantic search, AI workflow orchestration, secure enterprise integrations, and a modern React/.NET architecture.

---

## Product Positioning

Enterprise teams often work with contracts, technical documents, operational reports, procedures, emails, and internal knowledge scattered across different systems.

This platform provides a secure AI assistant that can:

- Understand uploaded enterprise documents
- Answer questions with source-grounded responses
- Extract risks, decisions, obligations, and action items
- Generate professional summaries and email drafts
- Invoke controlled backend tools
- Integrate with enterprise services such as Microsoft Graph, SQL Server, health check APIs, queues, and MCP-compatible tools

The assistant is not only a chat interface. It is an AI-enabled application layer that connects business documents, enterprise data, and operational tools.

---

## Key Capabilities

### AI Document Intelligence

- Document upload and parsing
- Document chunking and indexing
- Semantic search
- Retrieval-Augmented Generation
- Source-grounded answers
- Multi-document question answering
- Risk and obligation extraction
- Structured document summaries

### Conversational AI Assistant

- React-based AI assistant interface
- Streaming responses
- Multi-turn conversation support
- Context-aware document interaction
- Source citation rendering
- Tool execution feedback

### AI Orchestration

- Prompt orchestration
- AI Skills for reusable business capabilities
- Tool Calling through backend APIs
- Workflow execution for multi-step tasks
- Optional Agent-to-Agent collaboration for specialized processing

### Enterprise Integrations

- Microsoft Graph for Outlook, Calendar, and productivity workflows
- MCP Server for exposing application capabilities to external AI clients
- SQL Server / PostgreSQL integration
- RabbitMQ or queue monitoring integration
- System health check integration
- REST-based enterprise service connectors

### Enterprise Security

- OAuth2 / OpenID Connect
- JWT-based API protection
- User-scoped document access
- Authorization-aware retrieval
- API key and secret isolation
- Prompt injection mitigation
- Sensitive data handling

### Persistence and Storage

- Relational storage for business entities
- MongoDB for conversation state, workflow state, metadata, and flexible AI records
- Vector database for embeddings and semantic retrieval
- Audit-friendly storage design

### Platform Engineering

- ASP.NET Core backend
- React + TypeScript frontend
- Docker-ready architecture
- Structured logging
- Retry and timeout policies
- Rate limiting
- Observability and health checks

---

## Reference Architecture

```text
                              Enterprise AI Document Assistant

┌─────────────────────────────────────────────────────────────────────────────┐
│                                React Portal                                  │
│                                                                             │
│   AI Assistant UI  │  Document Center  │  Workflow Console  │  Admin Area    │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ASP.NET Core Web API                              │
│                                                                             │
│   Auth  │  Documents  │  Conversations  │  AI Gateway  │  Tool Gateway      │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           AI Orchestration Layer                             │
│                                                                             │
│   Prompt Orchestration  │  AI Skills  │  Tool Calling  │  Workflows         │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
┌─────────────────────────┐ ┌──────────────────┐ ┌──────────────────────────┐
│      RAG Pipeline        │ │   Enterprise      │ │       AI Models           │
│                          │ │   Tools           │ │                          │
│ Parse → Chunk → Embed    │ │ Graph / SQL / MQ  │ │ Chat / Embedding Models   │
│ Retrieve → Generate      │ │ Health / MCP      │ │ OpenAI / Azure OpenAI     │
└─────────────────────────┘ └──────────────────┘ └──────────────────────────┘
                    │                 │                 │
                    ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                                Data Layer                                    │
│                                                                             │
│   SQL Server / PostgreSQL  │  MongoDB  │  Vector Store  │  Object Storage    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Core User Flows

### Document Question Answering

```text
Upload document
      ↓
Extract text
      ↓
Chunk and embed content
      ↓
Store vectors and metadata
      ↓
Ask a question
      ↓
Retrieve relevant chunks
      ↓
Generate answer with citations
```

### Risk Analysis

```text
Select document
      ↓
Run Risk Analysis Skill
      ↓
Retrieve relevant sections
      ↓
Extract risks, obligations, deadlines, and missing information
      ↓
Return structured report
```

### Email Draft Generation

```text
Analyze document or conversation
      ↓
Generate professional response
      ↓
Validate tone and structure
      ↓
Create Outlook draft through Microsoft Graph
```

### Enterprise Tool Execution

```text
User asks an operational question
      ↓
AI identifies required tool
      ↓
Backend executes controlled function
      ↓
Tool result is injected into AI response
      ↓
Assistant returns grounded answer
```

---

## Technical Architecture

### Frontend

- React
- TypeScript
- Vite
- AI assistant interface
- Document upload UI
- Source citation panel
- Workflow status display

### Backend

- ASP.NET Core Web API
- Clean service-oriented architecture
- Authentication and authorization middleware
- AI orchestration services
- Document processing services
- Tool gateway services

### AI Layer

- Chat completion model
- Embedding model
- RAG pipeline
- Prompt orchestration
- AI Skills
- Tool Calling
- Workflow orchestration
- MCP-compatible tool exposure

### Storage Layer

- SQL Server or PostgreSQL for structured entities
- MongoDB for flexible AI-related state
- Vector store for semantic retrieval
- File/object storage for uploaded documents

### Integration Layer

- Microsoft Graph
- MCP Server
- SQL / REST enterprise APIs
- RabbitMQ or queue monitoring APIs
- Health check APIs

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

## Product Roadmap

### Phase 1 - Core Platform

- React portal foundation
- ASP.NET Core API foundation
- Authentication-ready architecture
- Basic AI assistant endpoint
- Initial document upload pipeline

### Phase 2 - Document Intelligence

- PDF / Word / text processing
- Chunking strategy
- Embedding generation
- Vector indexing
- RAG-based question answering
- Source citation support

### Phase 3 - AI Capabilities

- Prompt orchestration
- Document summary skill
- Risk analysis skill
- Email generation skill
- Structured AI output
- Conversation persistence

### Phase 4 - Enterprise Integrations

- Microsoft Graph integration
- SQL / REST tool connectors
- System health tool
- Queue status tool
- MCP Server for AI-compatible tool exposure

### Phase 5 - Workflow Orchestration

- Multi-step AI workflows
- AI Skills composition
- Tool execution pipeline
- Optional specialized agent collaboration
- Workflow state persistence

### Phase 6 - Enterprise Readiness

- Authorization-aware retrieval
- Rate limiting
- Retry and timeout policies
- Prompt injection protection
- Observability
- Health checks
- Docker-based deployment

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

- AI features must be grounded in enterprise use cases
- Documents and answers must remain traceable through citations
- Tool execution must be explicit, controlled, and auditable
- Retrieval must respect user authorization boundaries
- AI orchestration should remain separated from infrastructure and UI concerns
- The system should be extensible without becoming a collection of disconnected AI experiments

---

## Status

Product architecture initialized. Implementation will proceed through the roadmap phases.
