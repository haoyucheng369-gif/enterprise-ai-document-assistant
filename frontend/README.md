# Frontend

React workspace for Enterprise AI Document Assistant.

This README is scoped to the frontend only. Project status and roadmap live in the root README files.

## Current Scope

- Document list and selected document preview
- Upload drop zone for TXT, MD, PDF, and DOCX files
- File-type icons in the document list
- Right-side Assistant panel with streaming chat rendering
- Citation, tool result, and workflow result panels
- Document review workflow action from the workspace header
- API status connection to the ASP.NET Core backend
- Component-based structure with Tailwind CSS

## Structure

```text
src/
  api/
    chatApi.ts
    documentApi.ts
    statusApi.ts
    workspaceApi.ts
    workflowApi.ts
  components/
    assistant/
    documents/
    insights/
    layout/
  hooks/
  App.tsx
  index.css
  types.ts
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
