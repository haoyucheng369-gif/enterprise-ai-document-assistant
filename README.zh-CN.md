# Enterprise AI Document Assistant

一个聚焦型 React + ASP.NET Core AI 文档助手应用，用来串起现代 AI 应用开发的核心模块：Assistant UI、Prompt Orchestration、AI Gateway、RAG、Vector Search、Tool Calling、MCP、简单工作流编排和 Microsoft Graph 集成。

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
    UI->>API: POST /api/chat
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
| React Workspace | 用户工作区 | 文档列表、文档详情、右侧 Assistant、来源引用、工具结果 |
| ASP.NET Core API | 后端边界 | `/api/chat`、`/api/documents`、`/api/tools`、`/api/workflows` |
| AI Gateway | 模型访问层 | Chat 调用、Embedding 调用、model/token/latency 记录 |
| Document RAG | 基于来源的回答 | Upload、Parse、Chunk、Embed、Vector Search、Citations |
| Tool Gateway and Skills | 受控工具执行 | `SearchDocumentsTool`、`GetDocumentMetadataTool`、`SummarySkill`、`RiskAnalysisSkill` |
| MCP / Workflow / A2A | 扩展路径 | MCP `search_documents`、简单 Planner、一个 Workflow、可选双 Agent |

---

## 当前状态

- [x] React Workspace skeleton
- [ ] ASP.NET Core API skeleton
- [ ] Chat endpoint
- [ ] AI Gateway
- [ ] Document RAG
- [ ] Tool Gateway and Skills
- [ ] MCP and workflow extension

---

## 技术栈

| 领域 | 技术 |
|---|---|
| Frontend | React、TypeScript、Vite、Tailwind CSS |
| Backend | ASP.NET Core Web API |
| AI | OpenAI / Azure OpenAI，适配 Semantic Kernel 或 Microsoft.Extensions.AI 的设计 |
| Retrieval | Embeddings、Vector Store、Source Citations |
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

---

## 文档

- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)
- [English README](README.md)
