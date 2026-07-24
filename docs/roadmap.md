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
  -> real model provider
  -> classification skill
  -> resume review brief generation
  -> persistence
  -> RAG with citations
  -> grounded-answer guardrails
  -> document permission filtering
  -> rate limiting
  -> observability and cost tracking
  -> prompt versioning
  -> sensitive data redaction
  -> expanded harness checks
  -> intent classification and routing
  -> simple agent orchestration / A2A handoff
```

The implementation order intentionally brings Tool, MCP, Skill, Harness, and real model provider concepts earlier because they are common, concrete, and easier to understand before full RAG. Classification now uses the runtime provider selector so the same capability can run against local mock or a real model path.

---

## V1 Module Plan

V1 is organized around a small core path plus lightweight extension boundaries:

- React Workspace: document list, document detail, right-side AI Assistant, citation panel, tool result panel
- ASP.NET Core API: `/api/chat`, `/api/documents`, `/api/tools`, `/api/workflows`
- Prompt and AI Layer: prompt orchestration, conversation memory, structured output, validation, guardrails, AI Gateway
- Tool Gateway and Skills: `GetHealthStatusTool`, `GetDocumentMetadataTool`, `SummarySkill`, `RiskAnalysisSkill`, `EmailDraftSkill`, `ClassificationSkill`, `ResumeReviewSkill`
- Document Processing first, then RAG: upload, parse text, chunk first; embeddings, vector search, and grounded answers are staged next
- Persistence: conversation history, document metadata, workflow records, audit/tool records; MongoDB or relational storage can be selected later
- MCP, Harness, Workflow, and Integration Extension: MCP wrapper over registered tools, prompt/tool harnesses, document summary to risk analysis to email draft workflow, Microsoft Graph adapter boundary, optional later agent handoff

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

- Common enterprise system prompt defaults
- Prompt templates
- Task-specific prompt templates
- Prompt variables
- Prompt orchestration
- Output rules
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
- `EmailDraftSkill` composed from summary, risk analysis, metadata tool output, and AI generation
- `ResumeReviewSkill` for Markdown resume review brief generation
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
- Document skills can execute through either the local Mock path or the AI Gateway provider selected by the caller.
- Resume review demonstrates content generation as a separate Markdown brief contract instead of forcing long outputs into chat responses.

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
- Real OpenAI and Azure OpenAI routing is available through `OpenAiGateway`
- Summary, risk analysis, email draft, classification, workflow, and assistant chat can use the selected AI provider
- The React workspace can switch between local mock, OpenAI, and Azure OpenAI

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
- The backend can validate the file and return preview sections.
- Later retrieval steps have a clear entry point.

---

## Phase 8 - Workflow and Light Enterprise Integration

Objective: coordinate simple multi-step flows and connect one light enterprise integration before the full RAG slice.

Scope:

- Simple workflow: summarize document, identify risks, generate email draft
- One Microsoft Graph adapter boundary for an Outlook-style email draft scenario
- Optional Agent-to-Agent handoff: `DocumentAgent` to `EmailAgent`
- Basic workflow state persistence

Current V1 progress:

- `POST /api/workflows/document-review` runs summary, risk analysis, and email draft generation in sequence
- `POST /api/integrations/graph/email-draft` creates a mock Microsoft Graph email draft; OAuth-backed Graph calls are not wired yet

Expected outcome:

- The assistant can coordinate document analysis, tool execution, and business output generation in one small workflow.

---

## Phase 9 - Classification

Objective: add a common business AI capability that returns a validated structured result.

Scope:

- Document classification
- Priority and risk level classification
- Validated JSON response contracts
- Harness checks for fixed classification cases

Expected outcome:

- The assistant can classify documents in a format that backend code can consume safely.

Current V1 progress:

- `POST /api/skills/classification` returns category, priority, confidence, reason, signals, and sources
- The React workspace can run classification for the selected document with the active AI provider
- Harness checks validate the classification contract through the mock path

---

## Phase 10 - Real AI Gateway Provider

Objective: replace the mock model path with a configurable OpenAI or Azure OpenAI provider before adding more AI-heavy skills.

Scope:

- Provider configuration
- Chat model call
- Timeout and cancellation
- Safe request logging
- Token, latency, and provider metadata

Expected outcome:

- The same chat, skill, and workflow boundaries can run against a real model provider.

Current V1 progress:

- `OpenAiGateway` can be selected with `AiGateway:Provider=OpenAI` or `AiGateway:Provider=AzureOpenAI`
- The provider can also be selected from the React workspace for local comparison
- The mock provider remains available so local development works without secrets

---

## Phase 11 - Structured Extraction

Objective: extract business fields into validated JSON through the AI Gateway.

Scope:

- Contract field extraction
- Parties, renewal terms, liability cap, deadlines, and follow-up actions
- Validated JSON response contracts
- Harness checks for fixed extraction cases

Expected outcome:

- The assistant can extract key fields in a format that backend code can consume safely.

---

## Phase 12 - Persistence

Objective: replace selected in-memory stores with a small persistence boundary before retrieval features depend on stable state.

Scope:

- Docker Compose MongoDB baseline for local persistence work
- Conversation history
- Uploaded document metadata
- Workflow execution records
- Audit and tool execution records
- MongoDB or relational storage behind repository interfaces

Expected outcome:

- Restarting the API does not erase the core application state.
- Storage remains replaceable without changing controllers, skills, tools, or workflows.
- Local MongoDB can be inspected with MongoDB Compass at `mongodb://localhost:27017`.

---

## Phase 13 - Document RAG

Objective: enable source-grounded document question answering after ingestion and workflow concepts are in place.

Scope:

- Embedding generation through the AI Gateway
- Vector store integration
- Semantic retrieval
- RAG-based question answering
- Source citation support
- No-answer behavior when retrieval has no reliable context

Expected outcome:

- A user can upload a document and ask questions about it.
- Answers are grounded in retrieved document sections.
- The UI can show sources for generated answers.

---

## Phase 14 - Basic Security and Reliability

Objective: add the small production controls that make the assistant safer to discuss in enterprise interviews.

Scope:

- Basic document permission filtering
- Rate limiting for chat and model-backed endpoints
- Timeout and cancellation review
- Input and output guardrail expansion
- No-answer behavior when retrieved context is weak

Expected outcome:

- The assistant can explain why a user can or cannot access a document.
- Expensive or sensitive endpoints have a controlled request boundary.
- Grounded-answer behavior is explicit instead of relying on the model alone.

---

## Phase 15 - Observability and Cost Tracking

Objective: make AI operations visible and reviewable without adding a full monitoring platform.

Scope:

- Provider, model, token, latency, and cost estimate records
- Prompt name and prompt version on AI execution records
- Structured audit records for chat, skills, workflow, tool, and MCP calls
- Basic dashboard or endpoint for recent AI executions
- Redaction rules for sensitive prompt or document content
- Expanded harness checks for prompts, skills, tools, and workflows

Expected outcome:

- AI usage can be inspected and explained.
- The project demonstrates cost and latency awareness around model calls.
- Prompt changes can be traced by version without logging full sensitive content.

---

## Phase 16 - Intent Classification and Routing

Objective: route user requests to the right application path before introducing broader agent orchestration.

Scope:

- Classify user intent into known routes such as answer, summarize, classify, extract, risk analysis, email draft, tool lookup, or workflow
- Use `AiAgentPlanner` first when a real provider is available
- Fall back to deterministic rules in `SimpleAgentPlanner`
- Keep the route output structured and easy to validate

Current V1 progress:

- `RoutingAgentPlanner` tries AI-based intent routing first, then falls back to `SimpleAgentPlanner`.
- `SimpleAgentPlanner` classifies chat requests into known routes such as summary, risk analysis, email draft, classification, resume review, workflow, tool lookup, or normal chat.
- `ChatController` uses the selected route to execute the matching skill or workflow when the intent is clear.

Expected outcome:

- The assistant can choose a known skill, tool, or workflow path from a user request.
- Routing remains controlled instead of open-ended autonomous planning.

---

## Phase 17 - Simple Agent Orchestration / A2A Handoff

Objective: show a small agent-to-agent handoff without introducing broad autonomous behavior.

Scope:

- `CoordinatorAgent` selects the document review path
- `DocumentAgent` prepares summary, risk, classification, and extraction output
- `EmailAgent` receives structured handoff data and prepares follow-up content
- Harness check for the agent handoff

Expected outcome:

- The project demonstrates Agent Orchestration and A2A as a controlled application pattern.

---

## Deferred Scope

These items are outside the first implementation path and should be added only when they support a concrete product workflow:

- Authentication and authorization
- Authorization-aware RAG retrieval
- Prompt injection mitigation
- Real Microsoft Graph OAuth integration
- Hybrid search and semantic ranking
- GraphQL API surface
- Docker-based deployment
- CI foundation
