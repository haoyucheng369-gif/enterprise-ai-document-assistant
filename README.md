# Enterprise AI Document Assistant

An enterprise-oriented AI assistant built with **React**, **ASP.NET Core**, and modern Large Language Model technologies.

The goal of this project is not to build a simple chatbot, but to demonstrate how AI can be integrated into a real enterprise application: document analysis, RAG, tool calling, agent workflows, MCP, Microsoft Graph integration, MongoDB persistence, authentication, and observability.

---

## Project Vision

This project is designed as a practical learning and portfolio project for modern enterprise AI application development.

It combines:

- Traditional enterprise software architecture
- React frontend development
- ASP.NET Core Web API
- OAuth2 / OpenID Connect authentication
- LLM integration
- RAG-based document analysis
- Tool Calling
- Agent Workflow
- MCP Server
- Microsoft Graph integration
- MongoDB for conversation and memory storage
- Vector database for semantic search
- Enterprise-grade security and observability

---

## Main Use Case

A user uploads enterprise documents such as PDF, Word, or text files, then interacts with an AI assistant to:

- Summarize documents
- Ask questions based on uploaded content
- Extract risks or important points
- Generate professional email replies
- Create follow-up actions
- Search related knowledge
- Query backend tools such as system health, queue status, or metadata

Example questions:

```text
Summarize this document in 10 bullet points.
What are the key risks in this contract?
Generate a professional email reply based on this document.
Find all sections related to payment, delay, or termination.
Check the system health before generating the report.
```

---

## Features

### 1. Authentication and Security

- OAuth2 / OpenID Connect
- JWT authentication
- Protected REST APIs
- User-based document access
- Secure API integration
- Sensitive data handling

---

### 2. React Chatbot UI

- Modern React + TypeScript frontend
- Chatbox / chatbot interface
- Streaming AI responses
- Conversation history
- File upload
- Source citation display
- Tool execution result display

---

### 3. Document Upload and Analysis

Supported file types:

- PDF
- Word
- TXT / Markdown

Processing pipeline:

```text
Upload document
      ↓
Parse content
      ↓
Split into chunks
      ↓
Generate embeddings
      ↓
Store vectors
      ↓
Ask questions with RAG
```

---

### 4. RAG - Retrieval Augmented Generation

The assistant answers questions using uploaded documents as context.

Core concepts:

- Chunking
- Embedding
- Vector search
- Semantic retrieval
- Context injection
- Source citation
- Multi-document search

---

### 5. Vector Database

Used to store document embeddings for semantic search.

Possible implementations:

- PostgreSQL + pgvector
- Qdrant
- Chroma
- Azure AI Search

Initial implementation can start with a simple local vector store, then evolve to a production-ready vector database.

---

### 6. Prompt Engineering

The project includes prompt design for:

- Document summary
- Risk analysis
- Contract review
- Email generation
- Technical explanation
- Source-grounded answers
- Safe and structured responses

---

### 7. Tool Calling

The LLM can call backend tools instead of only generating text.

Example tools:

- `SearchDocuments`
- `GetDocumentMetadata`
- `GetSystemHealth`
- `GetQueueStatus`
- `QuerySqlServer`
- `GenerateEmailDraft`
- `CreateFollowUpTask`

This demonstrates how AI can interact with enterprise systems through controlled backend APIs.

---

### 8. AI Skills

Reusable business capabilities are implemented as AI skills.

Example skills:

- Document Summary Skill
- Risk Analysis Skill
- Contract Review Skill
- Email Generation Skill
- Technical Report Skill
- Incident Summary Skill

A skill is a reusable AI capability that combines prompt design, business rules, and optional tool calls.

---

### 9. Agent Workflow

The assistant can execute multi-step workflows.

Example:

```text
User request
   ↓
Document Agent
   ↓
Risk Analysis Skill
   ↓
Summary Agent
   ↓
Email Agent
   ↓
Microsoft Graph Tool
```

This demonstrates agent orchestration beyond a simple single-prompt chatbot.

---

### 10. MCP - Model Context Protocol

The project includes an MCP Server exposing backend capabilities to AI clients.

Example MCP tools:

- `search_documents`
- `get_document_by_id`
- `get_system_health`
- `get_queue_status`
- `search_users`

The objective is to understand how external AI clients can securely access application tools and context through MCP.

---

### 11. A2A - Agent-to-Agent Communication

A lightweight A2A demo can be added to show multiple agents cooperating.

Example:

```text
Document Agent
   ↓
Risk Agent
   ↓
Email Agent
```

The goal is not to over-engineer multi-agent systems, but to understand the concept and demonstrate a practical example.

---

### 12. Microsoft Graph Integration

The project can integrate with Microsoft Graph for enterprise productivity scenarios.

Example capabilities:

- Read emails
- Create email drafts
- Read calendar events
- Create calendar tasks
- Teams-related integration

Example workflow:

```text
Analyze uploaded document
      ↓
Generate email reply
      ↓
Create Outlook draft through Microsoft Graph
```

---

### 13. MongoDB

MongoDB is used for flexible document-oriented storage.

Possible use cases:

- Conversation history
- User sessions
- Agent memory
- Document metadata
- Tool execution logs
- Workflow state

The goal is to understand how document databases differ from relational databases and when they are useful in AI applications.

---

### 14. Enterprise Engineering Concerns

This project also covers enterprise-level concerns:

- Logging
- Error handling
- Retry policy
- Timeout handling
- Rate limiting
- Cost control
- API key protection
- Prompt injection mitigation
- RAG authorization filtering
- Observability
- Health checks
- Docker-based deployment

---

## Tech Stack

### Frontend

- React
- TypeScript
- Vite

### Backend

- ASP.NET Core
- Web API
- Minimal API or Controller-based API

### AI

- OpenAI or Azure OpenAI
- Semantic Kernel or Microsoft.Extensions.AI
- Embedding model
- Chat completion model

### Storage

- SQL Server or PostgreSQL
- MongoDB
- Vector database

### Integration

- Microsoft Graph
- MCP Server
- REST APIs

### Security

- OAuth2
- OpenID Connect
- JWT

### DevOps

- Docker
- GitHub Actions
- Structured logging

---

## Suggested Architecture

```text
React Frontend
     ↓
ASP.NET Core Web API
     ↓
Application Services
     ↓
AI Orchestration Layer
     ↓
LLM / Embedding Model
     ↓
Vector Database
     ↓
MongoDB / SQL Database
     ↓
External Tools: Microsoft Graph, MCP, System Health APIs
```

---

## Roadmap

### Phase 1 - Foundation

- Create React frontend
- Create ASP.NET Core backend
- Implement basic chatbot API
- Add project structure
- Add Docker support

### Phase 2 - Document RAG

- Upload documents
- Parse documents
- Chunk text
- Generate embeddings
- Store vectors
- Ask questions using RAG
- Return source citations

### Phase 3 - Tool Calling and Skills

- Implement backend tools
- Add document summary skill
- Add risk analysis skill
- Add email generation skill
- Add structured AI responses

### Phase 4 - MongoDB and Memory

- Store conversation history
- Store document metadata
- Store agent memory
- Store tool execution logs

### Phase 5 - Microsoft Graph Integration

- Implement OAuth flow
- Read user profile
- Create Outlook draft
- Read calendar events
- Add email-generation workflow

### Phase 6 - MCP and Agent Workflow

- Build MCP Server
- Expose backend tools through MCP
- Add basic Agent Workflow
- Add lightweight A2A demo

### Phase 7 - Enterprise Hardening

- Logging
- Rate limiting
- Retry and timeout handling
- Prompt injection mitigation
- RAG authorization filtering
- Observability dashboard
- Health checks

---

## Learning Objectives

This project is intended to cover the most important modern AI application concepts:

- Chatbot / Chatbox
- Prompt Engineering
- Embedding
- Vector Database
- RAG
- Tool Calling
- AI Skills
- Agent Workflow
- MCP
- A2A
- Microsoft Graph
- MongoDB
- OAuth2 / OIDC
- Enterprise AI security

---

## Status

Project initialized. Implementation will be developed progressively through roadmap issues.
