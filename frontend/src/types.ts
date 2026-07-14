export type DocumentItem = {
  id: string
  title: string
  type: string
  updatedAt: string
  status: string
  sections: DocumentSection[]
}

export type DocumentSection = {
  label: string
  title: string
  body: string
}

export type Message = {
  id: string
  role: 'user' | 'assistant'
  content: string
}

export type Citation = {
  id: string
  label: string
}

export type ToolResult = {
  name: string
  status: string
  description: string
}

export type ApiStatusResponse = {
  service: string
  environment: string
  apiVersion: string
  version: string
  aiProvider: string
  timeUtc: string
}

export type ApiConnectionState = 'loading' | 'connected' | 'unavailable'

export type WorkspaceResponse = {
  documents: DocumentItem[]
  messages: Message[]
  citations: Citation[]
  toolResult: ToolResult
}

export type DataConnectionState = 'loading' | 'loaded' | 'unavailable'

export type ChatRequest = {
  message: string
  documentId: string | null
  history: Message[]
}

export type ChatResponse = {
  message: Message
}
