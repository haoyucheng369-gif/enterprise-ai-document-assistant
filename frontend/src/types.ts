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
  confidence?: string
  citations?: string[]
  suggestedActions?: string[]
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

export type AiProviderSelection = 'Mock' | 'OpenAI' | 'AzureOpenAI'

export type WorkspaceResponse = {
  documents: DocumentItem[]
  messages: Message[]
  citations: Citation[]
  toolResult: ToolResult
}

export type DocumentUploadResponse = {
  id: string
  title: string
  type: string
  updatedAt: string
  status: string
  sizeBytes: number
  sections: DocumentSection[]
}

export type WorkflowStep = {
  name: string
  status: string
  detail: string
}

export type WorkflowRisk = {
  title: string
  severity: string
  source: string
  recommendation: string
}

export type WorkflowSummary = {
  documentId: string
  title: string
  summary: string
  keyPoints: string[]
  sources: string[]
}

export type WorkflowRiskAnalysis = {
  documentId: string
  title: string
  risks: WorkflowRisk[]
  missingInformation: string[]
}

export type WorkflowEmailDraft = {
  documentId: string
  subject: string
  body: string
  basedOn: string[]
  nextActions: string[]
}

export type ClassificationSkillRequest = {
  documentId: string
  aiProvider: AiProviderSelection
}

export type ClassificationSkillResponse = {
  documentId: string
  title: string
  category: string
  priority: string
  confidence: number
  reason: string
  signals: string[]
  sources: string[]
  provider: string
}

export type DocumentReviewWorkflowRequest = {
  documentId: string
  emailPurpose: string
  aiProvider?: AiProviderSelection
}

export type DocumentReviewWorkflowResponse = {
  workflowId: string
  status: string
  documentId: string
  steps: WorkflowStep[]
  summary: WorkflowSummary
  riskAnalysis: WorkflowRiskAnalysis
  emailDraft: WorkflowEmailDraft
}

export type ResumeReviewSkillRequest = {
  documentId: string
  instruction: string
  aiProvider: AiProviderSelection
}

export type ResumeReviewSkillResponse = {
  documentId: string
  title: string
  format: string
  content: string
  basedOn: string[]
  nextActions: string[]
  provider: string
}

export type DataConnectionState = 'loading' | 'loaded' | 'unavailable'

export type ChatRequest = {
  message: string
  documentId: string | null
  history: Message[]
  aiProvider: AiProviderSelection
}

export type ChatResponse = {
  message: Message
  structuredMessage: {
    answer: string
    confidence: string
    citations: string[]
    suggestedActions: string[]
  }
}
