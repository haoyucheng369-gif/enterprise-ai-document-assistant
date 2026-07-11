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
├── data/
│   └── workspaceData.ts
├── App.tsx
├── index.css
└── types.ts
```

## Commands

```bash
npm install
npm run dev
npm run build
npm run lint
```
