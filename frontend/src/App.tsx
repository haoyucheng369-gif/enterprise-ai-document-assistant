import { useEffect, useState } from 'react'
import { classifyDocument } from './api/classificationApi'
import { sendChatMessage } from './api/chatApi'
import { uploadDocument } from './api/documentApi'
import { generateResumeReview } from './api/resumeReviewApi'
import { runDocumentReviewWorkflow } from './api/workflowApi'
import { AssistantPanel } from './components/assistant/AssistantPanel'
import { DocumentNav } from './components/documents/DocumentNav'
import { DocumentWorkspace } from './components/documents/DocumentWorkspace'
import { useWorkspaceData } from './hooks/useWorkspaceData'
import type {
  AiProviderSelection,
  Citation,
  ClassificationSkillResponse,
  DocumentItem,
  DocumentReviewWorkflowResponse,
  Message,
  ResumeReviewSkillResponse,
} from './types'

const aiProviderStorageKey = 'enterprise-ai-document-assistant.aiProvider'

function getStoredAiProvider(): AiProviderSelection {
  const storedProvider = localStorage.getItem(aiProviderStorageKey)

  if (
    storedProvider === 'Mock'
    || storedProvider === 'OpenAI'
    || storedProvider === 'AzureOpenAI'
  ) {
    return storedProvider
  }

  return 'OpenAI'
}

function App() {
  const workspace = useWorkspaceData()
  const [messages, setMessages] = useState<Message[]>([])
  const [latestAssistantCitations, setLatestAssistantCitations] =
    useState<Citation[]>([])
  const [uploadedDocuments, setUploadedDocuments] = useState<DocumentItem[]>([])
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null)
  const [uploadState, setUploadState] = useState<'idle' | 'uploading' | 'failed'>('idle')
  const [workflowState, setWorkflowState] = useState<'idle' | 'running' | 'failed'>('idle')
  const [workflowResult, setWorkflowResult] =
    useState<DocumentReviewWorkflowResponse | null>(null)
  const [classificationState, setClassificationState] =
    useState<'idle' | 'running' | 'failed'>('idle')
  const [classificationResult, setClassificationResult] =
    useState<ClassificationSkillResponse | null>(null)
  const [reviewState, setReviewState] =
    useState<'idle' | 'running' | 'failed'>('idle')
  const [reviewResult, setReviewResult] =
    useState<ResumeReviewSkillResponse | null>(null)
  const [isSendingMessage, setIsSendingMessage] = useState(false)
  const [aiProvider, setAiProvider] =
    useState<AiProviderSelection>(getStoredAiProvider)

  useEffect(() => {
    if (workspace.data !== null) {
      setMessages(workspace.data.messages)
      setLatestAssistantCitations(workspace.data.citations)
    }
  }, [workspace.data])

  useEffect(() => {
    // Persist the last selected provider so repeated local testing does not require re-selecting it.
    localStorage.setItem(aiProviderStorageKey, aiProvider)
  }, [aiProvider])

  useEffect(() => {
    setClassificationResult(null)
    setClassificationState('idle')
    setReviewResult(null)
    setReviewState('idle')
  }, [selectedDocumentId])

  if (workspace.state === 'loading') {
    return (
      <main className="grid min-h-screen place-items-center bg-slate-100 text-slate-700">
        <div className="rounded-md border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          Loading workspace data...
        </div>
      </main>
    )
  }

  if (workspace.state === 'unavailable' || workspace.data === null) {
    return (
      <main className="grid min-h-screen place-items-center bg-slate-100 text-slate-700">
        <div className="rounded-md border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          Workspace data is unavailable. Start the API and refresh the page.
        </div>
      </main>
    )
  }

  const { documents, toolResult } = workspace.data
  const visibleDocuments = [...uploadedDocuments, ...documents]
  const selectedDocument =
    visibleDocuments.find((document) => document.id === selectedDocumentId)
    ?? visibleDocuments[0]

  async function handleUploadDocument(file: File) {
    setUploadState('uploading')

    try {
      const uploadedDocument = await uploadDocument(file)
      const documentItem: DocumentItem = {
        id: uploadedDocument.id,
        title: uploadedDocument.title,
        type: uploadedDocument.type,
        updatedAt: uploadedDocument.updatedAt,
        status: uploadedDocument.status,
        sections: uploadedDocument.sections,
      }

      setUploadedDocuments((currentDocuments) => [
        documentItem,
        ...currentDocuments,
      ])
      setSelectedDocumentId(documentItem.id)
      setUploadState('idle')
    } catch {
      setUploadState('failed')
    }
  }

  async function handleSendMessage(message: string) {
    const userMessage: Message = {
      id: `user-${crypto.randomUUID()}`,
      role: 'user',
      content: message,
    }

    const nextMessages = [...messages, userMessage]
    const assistantMessageId = `assistant-${crypto.randomUUID()}`
    const assistantMessage: Message = {
      id: assistantMessageId,
      role: 'assistant',
      content: '',
    }

    setMessages(nextMessages)
    setIsSendingMessage(true)

    try {
      setMessages([...nextMessages, assistantMessage])

      const response = await sendChatMessage({
        message,
        documentId: selectedDocument.id,
        history: messages,
        aiProvider,
      })

      const structuredMessage = response.structuredMessage
      setLatestAssistantCitations(
        structuredMessage.citations.map((citation, index) => ({
          id: `assistant-citation-${index + 1}`,
          label: citation,
        })),
      )
      setMessages((currentMessages) =>
        currentMessages.map((currentMessage) =>
          currentMessage.id === assistantMessageId
            ? {
                ...currentMessage,
                content: structuredMessage.answer,
                confidence: structuredMessage.confidence,
                citations: structuredMessage.citations,
                suggestedActions: structuredMessage.suggestedActions,
              }
            : currentMessage,
        ),
      )
    } catch {
      setMessages((currentMessages) => {
        const errorMessage =
          'The chat API is unavailable. Check that the backend is running and try again.'

        if (currentMessages.some((message) => message.id === assistantMessageId)) {
          return currentMessages.map((currentMessage) =>
            currentMessage.id === assistantMessageId
              ? { ...currentMessage, content: errorMessage }
              : currentMessage,
          )
        }

        return [
          ...currentMessages,
          {
            id: `assistant-error-${crypto.randomUUID()}`,
            role: 'assistant',
            content: errorMessage,
          },
        ]
      })
    } finally {
      setIsSendingMessage(false)
    }
  }

  async function handleRunWorkflow() {
    setWorkflowState('running')
    setWorkflowResult(null)

    try {
      const result = await runDocumentReviewWorkflow({
        documentId: selectedDocument.id,
        emailPurpose:
          'Ask the vendor to clarify renewal, liability, and service credit terms.',
        aiProvider,
      })

      setWorkflowResult(result)
      setWorkflowState('idle')
    } catch {
      setWorkflowState('failed')
    }
  }

  async function handleClassifyDocument() {
    setClassificationState('running')
    setClassificationResult(null)

    try {
      const result = await classifyDocument({
        documentId: selectedDocument.id,
        aiProvider,
      })

      setClassificationResult(result)
      setClassificationState('idle')
    } catch {
      setClassificationState('failed')
    }
  }

  async function handleGenerateResumeReview() {
    setReviewState('running')
    setReviewResult(null)

    try {
      const result = await generateResumeReview({
        documentId: selectedDocument.id,
        instruction:
          'Create a practical resume review brief that I can use with the original resume in ChatGPT.',
        aiProvider,
      })

      setReviewResult(result)
      setReviewState('idle')
    } catch {
      setReviewState('failed')
    }
  }

  return (
    <main className="grid h-screen overflow-hidden bg-slate-100 text-slate-900 lg:grid-cols-[272px_minmax(0,1fr)] xl:grid-cols-[288px_minmax(420px,1fr)_640px] 2xl:grid-cols-[300px_minmax(440px,1fr)_700px]">
      <DocumentNav
        documents={visibleDocuments}
        onSelectDocument={setSelectedDocumentId}
        onUploadDocument={handleUploadDocument}
        selectedDocumentId={selectedDocument.id}
        uploadState={uploadState}
      />
      <DocumentWorkspace
        citations={latestAssistantCitations}
        classificationResult={classificationResult}
        classificationState={classificationState}
        document={selectedDocument}
        onClassifyDocument={handleClassifyDocument}
        onGenerateResumeReview={handleGenerateResumeReview}
        onRunWorkflow={handleRunWorkflow}
        reviewResult={reviewResult}
        reviewState={reviewState}
        toolResult={toolResult}
        workflowResult={workflowResult}
        workflowState={workflowState}
      />
      <AssistantPanel
        aiProvider={aiProvider}
        isSending={isSendingMessage}
        messages={messages}
        onSelectAiProvider={setAiProvider}
        onSendMessage={handleSendMessage}
      />
    </main>
  )
}

export default App
