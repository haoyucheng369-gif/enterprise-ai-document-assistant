# Documents

Document modules own the document lifecycle inside the assistant.

- `Upload`: accepts files and creates document records.
- `Parsing`: extracts text and preview sections from uploaded files.
- `Providers`: exposes document and workspace data to controllers, skills, tools, and workflows.

RAG-specific retrieval code belongs in `Features/Rag`, not here.

