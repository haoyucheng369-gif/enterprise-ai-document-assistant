# Frontend

React Workspace for Enterprise AI Document Assistant.

## Current Scope

- Document list
- Document detail view
- Right-side AI Assistant shell
- Citation panel placeholder
- Tool result panel placeholder
- Tailwind CSS utility styling
- Component-based workspace structure
- API status connection to the ASP.NET Core backend
- Workspace data loaded from the ASP.NET Core backend
- Mock chat request/response through the ASP.NET Core backend
- Streaming chat response rendering in the Assistant panel

## Structure

```text
src/
├── components/
│   ├── assistant/
│   │   └── AssistantPanel.tsx
│   ├── documents/
│   │   ├── DocumentNav.tsx
│   │   ├── DocumentPreview.tsx
│   │   └── DocumentWorkspace.tsx
│   ├── insights/
│   │   ├── CitationPanel.tsx
│   │   └── ToolResultPanel.tsx
│   └── layout/
│       └── WorkspaceHeader.tsx
├── hooks/
│   ├── useWorkspaceData.ts
│   └── useApiStatus.ts
├── api/
│   ├── workspaceApi.ts
│   ├── chatApi.ts
│   └── statusApi.ts
├── App.tsx
├── index.css
└── types.ts
```

## Configuration

Create a local `.env` file when the backend API runs on a custom URL:

```bash
VITE_API_BASE_URL=http://localhost:5221
```

## Commands

```bash
npm install
npm run dev
npm run build
npm run lint
```
