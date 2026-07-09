# 企业级 AI 文档助手

一个基于 **React**、**ASP.NET Core** 和现代大语言模型技术构建的企业级 AI 应用项目。

本项目的目标不是做一个简单的 Chatbot，而是完整实践 AI 在企业应用中的落地方式：文档分析、RAG、Tool Calling、Agent Workflow、MCP、Microsoft Graph、MongoDB、认证、安全和可观测性。

---

## 项目定位

这个项目是一个面向企业场景的 AI 应用学习与展示项目。

它结合了：

- 传统企业级软件架构
- React 前端开发
- ASP.NET Core Web API
- OAuth2 / OpenID Connect 认证
- 大语言模型集成
- 基于 RAG 的文档分析
- Tool Calling
- Agent Workflow
- MCP Server
- Microsoft Graph 集成
- MongoDB 存储聊天记录和 Memory
- Vector Database 进行语义搜索
- 企业级安全和可观测性

---

## 核心业务场景

用户上传企业文档，例如 PDF、Word 或文本文件，然后通过 AI Assistant 完成：

- 文档总结
- 基于文档内容问答
- 提取风险点或重点信息
- 生成专业邮件回复
- 创建后续行动项
- 查询相关知识
- 调用后端工具，例如系统健康状态、队列状态、文档元数据等

示例问题：

```text
请用 10 个要点总结这份文档。
这份合同有哪些主要风险？
根据这份文档生成一封专业回复邮件。
找出所有和付款、延期、终止相关的条款。
在生成报告之前，先检查一下系统健康状态。
```

---

## 功能模块

### 1. 认证与安全

- OAuth2 / OpenID Connect
- JWT 认证
- 受保护的 REST API
- 基于用户的文档访问控制
- 安全的 API 集成
- 敏感数据处理

---

### 2. React Chatbot 界面

- React + TypeScript 前端
- Chatbox / Chatbot 对话界面
- Streaming AI 输出
- 多轮对话历史
- 文件上传
- 来源引用展示
- Tool Calling 执行结果展示

---

### 3. 文档上传与分析

支持文件类型：

- PDF
- Word
- TXT / Markdown

处理流程：

```text
上传文档
   ↓
解析内容
   ↓
切分 Chunk
   ↓
生成 Embedding
   ↓
存入向量数据库
   ↓
基于 RAG 进行问答
```

---

### 4. RAG - Retrieval Augmented Generation

AI 基于用户上传的文档内容进行回答。

核心概念：

- Chunking
- Embedding
- Vector Search
- Semantic Retrieval
- Context Injection
- Source Citation
- Multi-document Search

---

### 5. Vector Database

用于存储文档 Embedding，支持语义搜索。

可选实现：

- PostgreSQL + pgvector
- Qdrant
- Chroma
- Azure AI Search

初期可以从简单本地 Vector Store 开始，后续升级为更接近生产的向量数据库。

---

### 6. Prompt Engineering

项目会包含多个 Prompt 设计场景：

- 文档总结
- 风险分析
- 合同审核
- 邮件生成
- 技术解释
- 带来源引用的回答
- 安全且结构化的输出

---

### 7. Tool Calling

AI 不只是生成文本，还可以调用后端工具。

示例工具：

- `SearchDocuments`
- `GetDocumentMetadata`
- `GetSystemHealth`
- `GetQueueStatus`
- `QuerySqlServer`
- `GenerateEmailDraft`
- `CreateFollowUpTask`

这部分体现 AI 如何通过受控后端 API 与企业系统交互。

---

### 8. AI Skills

项目会把可复用业务能力封装成 AI Skills。

示例：

- 文档总结 Skill
- 风险分析 Skill
- 合同审核 Skill
- 邮件生成 Skill
- 技术报告 Skill
- Incident 总结 Skill

Skill 可以理解为：Prompt + 业务规则 + 可选工具调用 的可复用 AI 能力。

---

### 9. Agent Workflow

AI 可以执行多步骤工作流。

示例：

```text
用户请求
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

这部分体现的不是普通单轮 Chatbot，而是具备任务编排能力的 AI 应用。

---

### 10. MCP - Model Context Protocol

项目会实现一个 MCP Server，把后端能力暴露给 AI Client。

示例 MCP Tools：

- `search_documents`
- `get_document_by_id`
- `get_system_health`
- `get_queue_status`
- `search_users`

目标是理解外部 AI Client 如何通过 MCP 安全访问应用上下文和工具。

---

### 11. A2A - Agent-to-Agent Communication

项目后期可以加入一个轻量级 A2A Demo，展示多个 Agent 之间的协作。

示例：

```text
Document Agent
   ↓
Risk Agent
   ↓
Email Agent
```

这里不用过度复杂化，重点是理解多个 Agent 如何协作完成任务。

---

### 12. Microsoft Graph 集成

项目可以集成 Microsoft Graph，模拟企业办公自动化场景。

示例能力：

- 读取邮件
- 创建邮件草稿
- 读取日历
- 创建日程或任务
- Teams 相关集成

示例工作流：

```text
分析上传文档
   ↓
生成邮件回复
   ↓
通过 Microsoft Graph 创建 Outlook 草稿
```

---

### 13. MongoDB

MongoDB 用于灵活的文档型数据存储。

可能用途：

- 聊天记录
- 用户 Session
- Agent Memory
- 文档 Metadata
- Tool Execution Logs
- Workflow State

学习重点是理解 MongoDB 和关系型数据库的区别，以及它在 AI 应用中的适用场景。

---

### 14. 企业级工程能力

项目也会覆盖企业级落地必须考虑的问题：

- Logging
- Error Handling
- Retry Policy
- Timeout Handling
- Rate Limiting
- Cost Control
- API Key Protection
- Prompt Injection Mitigation
- RAG Authorization Filtering
- Observability
- Health Checks
- Docker 部署

---

## 技术栈

### Frontend

- React
- TypeScript
- Vite

### Backend

- ASP.NET Core
- Web API
- Minimal API 或 Controller-based API

### AI

- OpenAI 或 Azure OpenAI
- Semantic Kernel 或 Microsoft.Extensions.AI
- Embedding Model
- Chat Completion Model

### Storage

- SQL Server 或 PostgreSQL
- MongoDB
- Vector Database

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
- Structured Logging

---

## 推荐架构

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

- 创建 React 前端
- 创建 ASP.NET Core 后端
- 实现基础 Chatbot API
- 建立项目结构
- 加入 Docker 支持

### Phase 2 - Document RAG

- 上传文档
- 解析文档
- Chunk 切分
- 生成 Embedding
- 存储向量
- 基于 RAG 问答
- 返回来源引用

### Phase 3 - Tool Calling and Skills

- 实现后端工具
- 增加文档总结 Skill
- 增加风险分析 Skill
- 增加邮件生成 Skill
- 增加结构化 AI 响应

### Phase 4 - MongoDB and Memory

- 存储聊天记录
- 存储文档 Metadata
- 存储 Agent Memory
- 存储 Tool Execution Logs

### Phase 5 - Microsoft Graph Integration

- 实现 OAuth Flow
- 读取用户 Profile
- 创建 Outlook 草稿
- 读取日历
- 增加邮件生成工作流

### Phase 6 - MCP and Agent Workflow

- 构建 MCP Server
- 通过 MCP 暴露后端工具
- 增加基础 Agent Workflow
- 增加轻量级 A2A Demo

### Phase 7 - Enterprise Hardening

- Logging
- Rate Limiting
- Retry and Timeout Handling
- Prompt Injection Mitigation
- RAG Authorization Filtering
- Observability Dashboard
- Health Checks

---

## 学习目标

本项目计划覆盖现代 AI 应用开发中的主要概念：

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
- 企业级 AI 安全

---

## 当前状态

项目已初始化，后续会按照 Roadmap Issues 逐步实现。
