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
