# Architecture Overview

Enterprise AI Document Assistant is structured as a modular enterprise application with a clear separation between user experience, API services, AI orchestration, enterprise integrations, and data storage.

---

## High-Level Architecture

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

## Main Components

### React Portal

The frontend provides the user-facing AI assistant experience.

Responsibilities:

- Chat-based interaction
- Document upload
- Document search and source citation display
- Workflow status visualization
- Authentication flow integration

### ASP.NET Core Web API

The backend exposes secure application APIs.

Responsibilities:

- Authentication and authorization
- Document ingestion endpoints
- Conversation endpoints
- AI gateway endpoints
- Tool execution endpoints
- Integration endpoints

### AI Orchestration Layer

This layer coordinates AI-specific capabilities without coupling them directly to UI or infrastructure concerns.

Responsibilities:

- Prompt orchestration
- RAG execution
- AI Skills execution
- Tool Calling routing
- Workflow orchestration
- Model provider abstraction

### RAG Pipeline

The RAG pipeline converts enterprise documents into searchable AI context.

Responsibilities:

- Text extraction
- Chunking
- Embedding generation
- Vector indexing
- Context retrieval
- Citation tracking

### Enterprise Tool Gateway

The Tool Gateway exposes controlled backend capabilities to the AI layer.

Example tools:

- Document search
- Document metadata lookup
- System health status
- Queue status
- SQL query wrapper
- Microsoft Graph operations
- MCP-compatible tools

### Data Layer

The data layer separates different storage needs.

- Relational database: structured business entities
- MongoDB: conversations, metadata, workflow state, AI records
- Vector store: embeddings and semantic retrieval
- Object storage: original uploaded files

---

## Security Model

Security is designed around enterprise constraints.

Key principles:

- All APIs are protected by OAuth2 / OpenID Connect
- JWT claims are mapped to application permissions
- Document retrieval is filtered by user authorization
- Tool Calling is routed through controlled backend functions
- Secrets are never exposed to the frontend
- AI responses must not bypass backend authorization rules

---

## AI Execution Flow

```text
User message
   ↓
Conversation API
   ↓
AI Orchestration Layer
   ↓
Intent and context evaluation
   ↓
RAG retrieval and/or Tool Calling
   ↓
LLM response generation
   ↓
Citation and tool result formatting
   ↓
Streaming response to React UI
```

---

## Integration Strategy

Enterprise integrations are isolated behind application services and tool adapters.

This allows the AI layer to request capabilities without depending directly on external SDKs.

Examples:

- Microsoft Graph Adapter
- SQL Adapter
- Queue Monitoring Adapter
- Health Check Adapter
- MCP Tool Adapter

---

## Design Constraints

- The AI layer must remain provider-agnostic where possible.
- Tool Calling must be auditable.
- RAG retrieval must be authorization-aware.
- Prompt templates must be versioned and testable.
- Enterprise integrations must be isolated behind adapters.
- The frontend must not directly call AI providers or enterprise systems.
