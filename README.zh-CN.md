# Enterprise AI Document Assistant

**Enterprise AI Document Assistant** 是一个聚焦型 AI 文档助手应用，用 React、ASP.NET Core、RAG、AI 编排、Tool Calling、MCP 和 Microsoft Graph，把现代大语言模型应用开发中的核心概念串起来。

这个项目第一版的重点不是做复杂平台，而是先跑通一条端到端主线：对话入口、文档理解、向量检索、模型调用网关、后端工具调用、MCP 接口和一个简单工作流。

---

## 产品定位

企业团队经常需要处理合同、技术文档、运营报告、流程说明、邮件，以及分散在不同系统里的内部知识。

本应用第一版从一个 AI Assistant 开始，可以：

- 通过 React 界面进行对话
- 上传并处理业务文档
- 基于文档内容进行问答，并展示来源引用
- 通过 Prompt Orchestration 执行可复用的 AI 任务
- 通过 Tool Gateway 调用受控后端工具
- 集成一个轻量 Microsoft Graph 场景，例如 Outlook 或 Calendar
- 通过一个小型 MCP 兼容接口暴露部分能力
- 编排一个简单的多步骤 AI 工作流

它不是一个简单聊天页面，也不是一开始就完整企业平台；它是一个用 React + ASP.NET Core 串起 AI 应用核心概念的端到端项目。

---

## V1 范围

第一版应该是一条窄而完整的主线：

- React Chat 界面
- ASP.NET Core Conversation Endpoint
- 一到两个可复用任务的 Prompt Orchestration
- Structured Output、AI Output Validation 和轻量 Guardrails
- 用 AI Gateway 统一处理 Chat 和 Embedding 调用
- 文档上传、Chunking、Embedding、Vector Search
- 基础 Conversation Memory
- 带来源引用的 RAG 回答
- 一个小型 Tool Gateway，包含少量后端工具
- 一个最小 Microsoft Graph 集成
- 一个最小 MCP Server 和 Client 路径
- 一个简单 Agent Planner，用来选择已知工作流路径
- 一个组合文档分析和工具调用的简单 Workflow

这条主线跑通之前，其他企业能力都保持轻量。

---

## V1 模块

第一版按 6 个务实模块组织，而不是一开始拆成很大的企业平台。

### 1. React Workspace

- 文档列表
- 文档详情
- 右侧 AI Assistant
- Citation 面板
- Tool Result 面板

### 2. ASP.NET Core API

- `/api/chat`
- `/api/documents`
- `/api/tools`
- `/api/workflows`

### 3. AI Gateway

- 统一调用 Chat Model
- 统一调用 Embedding Model
- 记录 model、token、latency
- 只做简单 Provider 配置，先不做复杂 fallback

### 4. Document RAG

- Upload
- Parse Text
- Chunk
- Embed
- Vector Search
- Answer with Citations

### 5. Tool Gateway and Skills

- `SearchDocumentsTool`
- `GetDocumentMetadataTool`
- `CreateEmailDraftTool` 或 `GetHealthStatusTool`
- `SummarySkill`
- `RiskAnalysisSkill`

### 6. MCP, Workflow, and A2A Extension

- MCP Server 暴露 `search_documents`
- Workflow：总结文档、识别风险、生成邮件草稿
- 可选 A2A 路径：`DocumentAgent` 和 `EmailAgent`

---

## 核心能力

### AI Assistant

- Conversation API
- Streaming 响应
- 多轮上下文
- React Assistant 界面
- 来源引用和工具结果展示
- 基础 Conversation Memory

### Prompt Orchestration

- Prompt Templates
- 运行时变量
- 结构化输出
- AI 输出校验
- Guardrails
- 可复用 AI Skills

### Document Intelligence

- 文档上传
- 文本解析
- Metadata 管理
- Chunking
- 文档总结
- 风险与义务提取

### AI Gateway

- 模型 Provider 抽象
- 面向 OpenAI / Azure OpenAI 的设计
- 适配 Microsoft.Extensions.AI 和 Semantic Kernel 的架构
- Retry / Timeout
- 请求日志
- 模型配置

### RAG

- Embedding 生成
- 文档更新时的 Embedding Lifecycle
- Vector Database 集成
- 语义搜索
- 上下文检索
- 来源引用追踪
- 基于文档证据的问答

### Tool Gateway

- 后端工具注册
- Tool Calling 执行
- 输入参数校验
- 执行记录
- 企业 API Adapter

### Persistence

- 关系型数据库保存结构化应用实体
- 文档 Metadata
- 对话历史
- 工作流状态
- 可选 MongoDB 或 JSON 存储保存灵活 AI 记录

### Enterprise Integration

- Microsoft Graph：Outlook 或 Calendar 场景
- REST API 连接器模式
- SQL 业务数据查询模式
- Health Check Tool

### MCP Interface

- MCP Server
- 基于应用服务的 MCP Tools
- 一条小型外部 AI Client 访问路径

### Workflow Orchestration

- 简单 Agent Planner
- AI Skill 组合
- 一个简单多步骤文档工作流
- Tool Execution Pipeline
- A2A 作为可选扩展模式

---

## 参考架构

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

## 核心流程

### 文档问答

```text
上传文档
   ↓
提取文本和 Metadata
   ↓
Chunking + Embedding
   ↓
保存向量和文档 Metadata
   ↓
用户提问
   ↓
检索相关片段
   ↓
生成带来源引用的回答
```

### Tool Calling

```text
用户提出动作请求或企业数据问题
   ↓
Assistant 选择后端工具
   ↓
Tool Gateway 校验参数
   ↓
后端执行受控操作
   ↓
工具结果进入 AI 响应
```

### AI Workflow

```text
选择文档
   ↓
Agent Planner 选择简单执行计划
   ↓
执行总结、风险分析或邮件草稿 Skill
   ↓
检索支持证据
   ↓
生成结构化结果
   ↓
可选生成后续邮件或任务
```

---

## 技术栈

### Frontend

- React
- TypeScript
- Vite
- AI Assistant UI
- 文档上传 UI
- 来源引用和工具结果面板

### Backend

- ASP.NET Core Web API
- 面向服务的应用结构
- Conversation Endpoints
- Document Endpoints
- AI Gateway
- Tool Gateway
- Integration Adapters

### AI Application Layer

- Chat Completion
- Embeddings
- Prompt Orchestration
- Structured Output
- AI Output Validation
- Guardrails
- RAG
- Tool Calling
- Simple Agent Planner
- MCP 兼容工具暴露
- Workflow Orchestration
- 可选 A2A 模式

### Storage

- SQL Server 或 PostgreSQL
- Vector Store
- 文件或对象存储
- 可选 MongoDB 保存灵活 AI 状态

### Integrations

- Microsoft Graph
- REST APIs
- SQL-backed services
- Health Check APIs
- MCP Clients and Servers

---

## 仓库结构

```text
enterprise-ai-document-assistant/
│
├── frontend/                  # React + TypeScript application
├── backend/                   # ASP.NET Core Web API
│   ├── src/
│   └── tests/
│
├── docs/
│   ├── architecture.md        # 架构说明
│   ├── roadmap.md             # 产品路线图
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

## 路线图

### Phase 1 - Full-Stack Foundation

- React 前端基础
- ASP.NET Core Web API 基础
- 基础 AI Assistant Endpoint
- Streaming 对话响应
- 配置结构

### Phase 2 - Prompt and AI Gateway

- Prompt Orchestration
- 结构化输出
- AI Output Validation 和 Guardrails
- AI Gateway 抽象
- Model Provider 配置

### Phase 3 - Document Intelligence and RAG

- 文档上传与解析
- Chunking
- Embedding 生成
- Embedding Lifecycle
- Vector Search
- 带来源引用的文档问答

### Phase 4 - Tool Gateway and Light Enterprise Integration

- 通过后端服务执行 Tool Calling
- Microsoft Graph 集成
- SQL / REST Tool Adapters
- Health Check Tool
- 工具执行记录

### Phase 5 - MCP and Simple Workflow Orchestration

- MCP Server
- 基于 MCP 的应用工具
- AI Skills
- Simple Agent Planner
- 一个多步骤工作流
- 可选 A2A 协作

---

## 本地开发

### 前置环境

- Node.js LTS
- .NET SDK
- Docker Desktop
- Git

### Clone 仓库

```bash
git clone https://github.com/haoyucheng369-gif/enterprise-ai-document-assistant.git
cd enterprise-ai-document-assistant
code .
```

后续实现会逐步放入 `frontend/`、`backend/`、`docs/` 和 `infrastructure/`。

---

## 设计原则

- 范围保持务实，聚焦一条端到端 AI 应用主线。
- 使用产品化命名，避免临时性或课堂式表达。
- 前端、后端、AI 编排和企业集成保持清晰分层。
- 模型访问统一经过 AI Gateway。
- 企业动作统一经过 Tool Gateway。
- 文档回答必须可追溯、有来源引用。
- 在适合 .NET 后端的场景下，优先采用 Semantic Kernel 和 Microsoft.Extensions.AI 友好的架构。

---

## 当前状态

架构和路线图已初始化，后续将按照路线图逐步实现。
