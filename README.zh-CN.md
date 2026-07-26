# Enterprise AI Document Assistant

一个面向企业应用场景的 React + ASP.NET Core AI 文档助手，用来串起现代 AI 应用里的核心模块：Assistant UI、Prompt Orchestration、AI Gateway、文档解析、结构化输出、Safety Classifier、Tool Gateway、MCP、Skills、Workflow、Planner、MongoDB 持久化，以及后续 RAG 检索。

V1 保持小而完整：先跑通一条端到端文档助手主线，再逐步加入检索、向量搜索和权限控制。

---

## V1 架构

```mermaid
flowchart LR
    user[User] --> ui[React Workspace]

    subgraph frontend[Frontend]
        ui --> docs[Document Workspace]
        ui --> assistant[Assistant Panel]
        ui --> insights[Preview + Classification + Workflow + Citations + Tools]
    end

    assistant --> api[ASP.NET Core API]
    docs --> api

    subgraph backend[Backend]
        api --> safety[Safety Classifier + Guardrails]
        safety --> planner[Agent Planner]
        planner --> skills[Skills]
        planner --> workflows[Workflow]
        api --> gateway[AI Gateway]
        api --> tools[Tool Gateway]
        api --> mongo[MongoDB]
    end

    subgraph extensions[Extension Points]
        gateway --> models[OpenAI / Azure OpenAI / Mock]
        tools --> mcp[MCP Surface]
        workflows --> graph[Microsoft Graph Adapter]
    end
```

---

## 当前状态

### 已完成

- [x] React workspace：文档列表、上传区、文档 tabs、右侧 Assistant
- [x] ASP.NET Core controller API、Swagger、ProblemDetails
- [x] 后端驱动 workspace 数据
- [x] 文档上传、文本解析、preview chunk
- [x] MongoDB 持久化 uploaded document metadata 和 parsed sections
- [x] Chat endpoint：prompt orchestration、conversation memory、structured output
- [x] Input Guardrails：规则安全分类、可选 AI 安全分类、fallback
- [x] Guardrails：prompt injection、unauthorized data 基础拦截
- [x] AI Gateway：Mock、OpenAI、Azure OpenAI provider selection
- [x] Skills：classification、summary、risk analysis、email draft、resume review
- [x] Workflow：summary -> risk analysis -> email draft
- [x] Tool Gateway：health 和 document metadata tools
- [x] MCP surface：暴露已注册 tools
- [x] Agent Planner：AI routing + deterministic fallback
- [x] Microsoft Graph adapter mock
- [x] In-memory audit logging
- [x] Harness checks

### 下一步主线

- [ ] Embeddings
- [ ] Vector Search
- [ ] RAG Answer with Citations
- [ ] No-answer Guardrail
- [ ] Basic Document Permission Filtering

### 轻量增强

- [ ] Conversation / workflow / audit persistence
- [ ] Rate limiting
- [ ] Observability and cost tracking
- [ ] Prompt versioning
- [ ] Sensitive data redaction
- [ ] Expanded harness checks
- [ ] Simple Agent Orchestration / A2A handoff

---

## 当前核心流程

```text
User message
  -> React Assistant
  -> ASP.NET Core /api/chat
  -> Safety Classifier
  -> Guardrails
  -> Agent Planner
  -> Skill / Workflow / normal chat
  -> AI Gateway
  -> Structured Output Validation
  -> React UI
```

---

## 本地开发

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Backend:

```bash
cd backend
dotnet run --project src/EnterpriseAiDocumentAssistant.Api
```

MongoDB:

```bash
docker compose up -d mongodb
docker compose ps
```

MongoDB Compass:

```text
mongodb://localhost:27017
```

本地 AI provider 配置放在：

```text
backend/src/EnterpriseAiDocumentAssistant.Api/appsettings.Local.json
```

这个文件已被 Git ignore。

---

## 文档

- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)
- [English README](README.md)
