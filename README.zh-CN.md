# Enterprise AI Document Assistant

**Enterprise AI Document Assistant** 是一个面向企业场景的 AI 文档助手，用于帮助组织通过大语言模型分析、搜索、理解并处理业务文档。

该平台结合了文档智能、RAG、语义搜索、AI 工作流编排、安全企业集成，以及现代 React/.NET 架构。

---

## 产品定位

企业团队经常需要处理合同、技术文档、运营报告、流程说明、邮件和分散在不同系统中的内部知识。

本平台提供一个安全的 AI Assistant，可以：

- 理解用户上传的企业文档
- 基于来源内容进行问答
- 提取风险、决策、义务和行动项
- 生成专业总结和邮件草稿
- 调用受控的后端工具
- 集成 Microsoft Graph、SQL Server、Health Check API、消息队列和 MCP 兼容工具

它不是一个简单聊天界面，而是一个连接业务文档、企业数据和运维工具的 AI 应用层。

---

## 核心能力

### AI 文档智能

- 文档上传与解析
- 文档 Chunking 与索引
- 语义搜索
- Retrieval-Augmented Generation
- 带来源依据的回答
- 多文档问答
- 风险与义务提取
- 结构化文档总结

### AI Assistant 对话入口

- React AI Assistant 界面
- Streaming 响应
- 多轮对话
- 基于上下文的文档交互
- 来源引用展示
- Tool Calling 执行结果展示

### AI 编排层

- Prompt 编排
- 可复用 AI Skills
- 后端 Tool Calling
- 多步骤工作流执行
- 可选的 Agent-to-Agent 专项协作

### 企业系统集成

- Microsoft Graph：Outlook、Calendar、企业办公流程
- MCP Server：向外部 AI Client 暴露应用能力
- SQL Server / PostgreSQL 集成
- RabbitMQ 或队列监控集成
- 系统 Health Check 集成
- REST 企业服务连接器

### 企业安全

- OAuth2 / OpenID Connect
- JWT API 保护
- 基于用户的文档访问控制
- 带授权过滤的 RAG 检索
- API Key 和 Secret 隔离
- Prompt Injection 防护
- 敏感数据处理

### 持久化与存储

- 关系型数据库保存业务实体
- MongoDB 保存对话状态、工作流状态、Metadata 和灵活 AI 记录
- Vector Database 保存 Embedding 和语义检索索引
- 文件/对象存储保存上传文档

### 平台工程能力

- ASP.NET Core 后端
- React + TypeScript 前端
- Docker-ready 架构
- Structured Logging
- Retry / Timeout 策略
- Rate Limiting
- Observability 与 Health Checks

---

## 参考架构

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

## 核心流程

### 文档问答

```text
上传文档
   ↓
提取文本
   ↓
Chunking + Embedding
   ↓
存储向量与 Metadata
   ↓
用户提问
   ↓
检索相关片段
   ↓
生成带来源引用的回答
```

### 风险分析

```text
选择文档
   ↓
执行 Risk Analysis Skill
   ↓
检索相关段落
   ↓
提取风险、义务、截止日期和缺失信息
   ↓
返回结构化报告
```

### 邮件草稿生成

```text
分析文档或对话上下文
   ↓
生成专业回复
   ↓
校验语气和结构
   ↓
通过 Microsoft Graph 创建 Outlook 草稿
```

### 企业工具调用

```text
用户提出运营类问题
   ↓
AI 识别所需工具
   ↓
后端执行受控函数
   ↓
工具结果注入 AI 响应
   ↓
Assistant 返回有依据的回答
```

---

## 技术架构

### Frontend

- React
- TypeScript
- Vite
- AI Assistant 界面
- 文档上传 UI
- 来源引用面板
- 工作流状态展示

### Backend

- ASP.NET Core Web API
- 清晰的服务分层架构
- 认证与授权中间件
- AI 编排服务
- 文档处理服务
- Tool Gateway 服务

### AI Layer

- Chat Completion Model
- Embedding Model
- RAG Pipeline
- Prompt Orchestration
- AI Skills
- Tool Calling
- Workflow Orchestration
- MCP 兼容工具暴露

### Storage Layer

- SQL Server 或 PostgreSQL 保存结构化实体
- MongoDB 保存灵活 AI 状态
- Vector Store 做语义检索
- 文件/对象存储保存上传文档

### Integration Layer

- Microsoft Graph
- MCP Server
- SQL / REST 企业 API
- RabbitMQ 或队列监控 API
- Health Check API

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

## 产品路线图

### Phase 1 - Core Platform

- React Portal 基础
- ASP.NET Core API 基础
- 认证预留架构
- 基础 AI Assistant Endpoint
- 初始文档上传管线

### Phase 2 - Document Intelligence

- PDF / Word / Text 处理
- Chunking 策略
- Embedding 生成
- Vector Indexing
- RAG 文档问答
- 来源引用支持

### Phase 3 - AI Capabilities

- Prompt 编排
- 文档总结 Skill
- 风险分析 Skill
- 邮件生成 Skill
- 结构化 AI 输出
- 对话持久化

### Phase 4 - Enterprise Integrations

- Microsoft Graph 集成
- SQL / REST Tool Connectors
- 系统 Health Tool
- 队列状态 Tool
- MCP Server 暴露 AI 兼容工具

### Phase 5 - Workflow Orchestration

- 多步骤 AI 工作流
- AI Skills 组合
- Tool Execution Pipeline
- 可选专项 Agent 协作
- 工作流状态持久化

### Phase 6 - Enterprise Readiness

- 授权感知的 RAG 检索
- Rate Limiting
- Retry / Timeout 策略
- Prompt Injection 防护
- Observability
- Health Checks
- Docker 部署

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

- AI 能力必须服务真实企业场景
- 文档回答必须可追溯、有来源引用
- Tool Calling 必须显式、受控、可审计
- RAG 检索必须遵守用户权限边界
- AI 编排层应与 UI、基础设施保持清晰分离
- 系统应保持可扩展，而不是堆砌彼此孤立的 AI 实验功能

---

## 当前状态

产品架构已初始化，后续将按照路线图逐步实现。
