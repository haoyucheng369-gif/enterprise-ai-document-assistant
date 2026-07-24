# Enterprise AI Document Assistant

一个面向生产场景的 React + ASP.NET Core AI 文档助手应用，用来串起现代 AI 应用开发的核心模块：Assistant UI、Prompt Orchestration、AI Gateway、RAG、Vector Search、Tool Calling、MCP、简单工作流编排和 Microsoft Graph 集成。

V1 保持小而完整：先跑通一条端到端文档助手主线，再逐步加深。

---

## V1 架构

```mermaid
flowchart LR
    user[User] --> ui[React Workspace]

    subgraph frontend[Frontend]
        ui --> docs[Document View]
        ui --> assistant[Right-side Assistant]
        ui --> panels[Citations + Tool Results]
    end

    assistant --> api[ASP.NET Core API]
    docs --> api

    subgraph backend[Backend]
        api --> gateway[AI Gateway]
        api --> tools[Tool Gateway]
        api --> workflows[Workflow API]
        gateway --> prompts[Prompt Orchestration]
        gateway --> rag[Document RAG]
        workflows --> planner[Simple Agent Planner]
    end

    subgraph ai[AI + Data]
        rag --> vector[Vector Store]
        rag --> files[Document Storage]
        gateway --> models[OpenAI / Azure OpenAI]
        tools --> msgraph[Microsoft Graph]
        tools --> mcp[MCP Tools]
    end
```

---

## V1 流程

```mermaid
sequenceDiagram
    participant U as User
    participant UI as React Workspace
    participant API as ASP.NET Core API
    participant AI as AI Gateway
    participant RAG as Document RAG
    participant Tools as Tool Gateway

    U->>UI: Ask about a document
    UI->>API: POST /api/chat/stream
    API->>AI: Build prompt + route request
    AI->>RAG: Retrieve relevant chunks
    RAG-->>AI: Context + citations
    AI->>Tools: Optional controlled tool call
    Tools-->>AI: Tool result
    AI-->>API: Answer with citations
    API-->>UI: Stream response
```

---

## V1 模块

| 模块 | 作用 | 第一版范围 |
|---|---|---|
| React Workspace | 用户工作区 | 文档列表、文档工作区 tabs、右侧 Assistant、来源引用、工具结果 |
| ASP.NET Core API | 后端边界 | `/api/chat`、`/api/documents`、`/api/tools`、`/api/workflows` |
| Prompt and AI Layer | Prompt / AI 控制层 | Prompt Orchestration、Structured Output、Validation、Guardrails、AI Gateway |
| Tool Gateway and Skills | 受控工具执行 | `SearchDocumentsTool`、`GetDocumentMetadataTool`、`SummarySkill`、`RiskAnalysisSkill`、`EmailDraftSkill` |
| Document RAG | 基于来源的回答 | Upload、Parse、Chunk、Embed、Vector Search、Citations |
| Persistence | 应用状态存储 | Conversation history、document metadata、workflow records、audit/tool records；MongoDB 保持可选 |
| MCP / Harness / Workflow / Agent Orchestration | 扩展路径 | MCP 暴露已有 Tool、Prompt/Tool Harness、一个 Workflow、协调者调度 Agent、可选 A2A 交接 |

---

## 当前状态

- [x] React Workspace skeleton
- [x] ASP.NET Core API skeleton
- [x] Backend-driven workspace data
- [x] Chat endpoint
- [x] Streaming chat response
- [x] Prompt orchestration
- [x] Common enterprise prompt defaults
- [x] Structured output validation
- [x] Simple guardrails
- [x] Tool Gateway
- [x] First tools
- [x] MCP Server
- [x] Prompt and Tool Harness
- [x] SummarySkill
- [x] RiskAnalysisSkill
- [x] EmailDraftSkill
- [x] Conversation Memory
- [x] Simple Agent Planner
- [x] Audit Logging
- [x] AI Gateway
- [x] Document Upload
- [x] Text Parsing and Chunking
- [x] Workflow
- [x] Microsoft Graph Integration
- [x] Real AI Gateway Provider
- [x] ClassificationSkill
- [x] AI-backed Summary, Risk Analysis, and Email Draft skills
- [x] ResumeReviewSkill for Markdown resume review brief generation
- [x] Expanded skill prompt context for longer resume inputs
- [x] React 工作区内的模型 Provider 切换
- [x] Document workspace tabs for preview, classification, workflow, review, citations, and tool context

### Build Next

- [ ] Persistence
- [ ] MongoDB 或关系型数据库持久化 conversation / document state
- [ ] Embeddings
- [ ] Vector Search
- [ ] RAG Answer with Citations
- [ ] No-answer Guardrail
- [ ] 基础文档权限过滤

### Build Lightly

- [ ] Rate limiting
- [ ] Observability and cost tracking
- [ ] Prompt versioning
- [ ] AI 日志敏感数据脱敏
- [ ] Prompt、Skill、Tool、Workflow 的 harness 检查扩展
- [ ] Simple Agent Orchestration / A2A handoff

---

## Next Implementation Order

```text
Persistence
  -> Embeddings
  -> Vector Search
  -> RAG Answer with Citations
  -> No-answer Guardrail
  -> Basic Document Permission Filtering
  -> Rate Limiting
  -> Observability and Cost Tracking
  -> Prompt Versioning
  -> Sensitive Data Redaction
  -> Expanded Harness Checks
  -> Simple Agent Orchestration / A2A Handoff
```

`Build Next` is the main delivery path. `Build Lightly` items stay small and are implemented only when they strengthen the application architecture.

### Deferred Scope

- Hybrid search and semantic ranking
- Real Microsoft Graph OAuth integration
- GraphQL API surface
- CI and deployment hardening

---

## 技术栈

| 领域 | 技术 |
|---|---|
| Frontend | React、TypeScript、Vite、Tailwind CSS |
| Backend | ASP.NET Core Web API |
| AI | OpenAI / Azure OpenAI，适配 Semantic Kernel 或 Microsoft.Extensions.AI 的设计 |
| Retrieval | Embeddings、Vector Store、Source Citations |
| Persistence | 先使用 in-memory，后续可接 MongoDB 或关系型数据库 |
| Integration | Microsoft Graph、REST APIs、MCP |

---

## 本地开发

```bash
git clone https://github.com/haoyucheng369-gif/enterprise-ai-document-assistant.git
cd enterprise-ai-document-assistant
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Backend local AI provider settings can be placed in:

```text
backend/src/EnterpriseAiDocumentAssistant.Api/appsettings.Local.json
```

Edit that local file with your provider, model, and API key. The local file is ignored by Git.

Local MongoDB can be started from the repository root:

```bash
docker compose up -d mongodb
docker compose ps
```

MongoDB Compass connection string:

```text
mongodb://localhost:27017
```

MongoDB data is stored in a Docker named volume. Use `docker compose down -v` only when you intentionally want to delete the local database volume.

---

## 文档

- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)
- [English README](README.md)
