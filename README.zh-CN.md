# Enterprise AI Document Assistant

一个面向企业应用场景的 React + ASP.NET Core AI 文档助手，用来串起现代 AI 应用里的核心模块：Assistant UI、Prompt Orchestration、AI Gateway、文档解析、结构化输出、Safety Classifier、Tool Gateway、MCP、Skills、Workflow、Planner、MongoDB，以及基于 Qdrant 的 RAG 检索。

V1 保持小而完整：端到端文档问答和向量检索主线已经跑通，后续只补持久化、基础权限和轻量运行保障。

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
        safety --> intent[Intent Classifier]
        intent --> planner[Agent Planner]
        planner --> skills[Skills]
        planner --> workflows[Workflow]
        api --> gateway[AI Gateway]
        api --> tools[Tool Gateway]
        api --> mongo[MongoDB]
        api --> rag[RAG]
        rag --> vectors[Qdrant / In-memory Vector Store]
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
- [x] 单轮原生 Tool Calling：模型选择 -> Tool Gateway 校验执行 -> 模型生成最终回答
- [x] MCP surface：暴露已注册 tools
- [x] Intent Classifier、Agent Planner、Executor 职责拆分：AI 分类 + 规则 fallback
- [x] Microsoft Graph adapter mock
- [x] In-memory audit logging
- [x] Harness checks
- [x] RAG：embedding、语义检索、引用和 no-answer threshold
- [x] Qdrant 向量持久化与 in-memory 替代实现

### 下一步主线

- [ ] MongoDB 对话持久化
- [ ] 基础文档权限过滤
- [ ] 简单 Agent Orchestration / A2A
- [ ] 轻量限流、日志和成本记录

### 轻量增强

- [ ] Prompt versioning
- [ ] Sensitive data redaction
- [ ] Expanded harness checks

---

## 当前核心流程

```text
User message
  -> React Assistant
  -> ASP.NET Core /api/chat
  -> Safety Classifier
  -> Guardrails
  -> Intent Classifier
  -> Agent Planner
  -> Skill / Workflow / Tool Calling / RAG chat
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
