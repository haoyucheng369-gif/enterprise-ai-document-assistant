# RAG

RAG modules connect parsed document chunks to grounded assistant answers.

Current areas:

- `IEmbeddingGateway`: creates vectors for document chunks and user questions.
- `RoutingEmbeddingGateway`: uses deterministic local embeddings for Mock and OpenAI/Azure OpenAI embeddings for real providers.
- `IVectorStore`: keeps vector storage replaceable.
- `InMemoryVectorStore`: first-version cosine similarity search.
- `RagService`: indexes uploaded chunks, rejects weak matches with a configurable similarity threshold, and returns citations.
- No-answer handling stops model generation when retrieval cannot provide reliable document evidence.

Replacement path:

- Keep `RagService`.
- Replace `InMemoryVectorStore` with a later `QdrantVectorStore`, MongoDB Atlas Vector Search, or Azure AI Search implementation.
