import { useEffect, useState } from 'react'
import { streamChatMessage } from './api/chatApi'
import { uploadDocument } from './api/documentApi'
import { runDocumentReviewWorkflow } from './api/workflowApi'
import { AssistantPanel } from './components/assistant/AssistantPanel'
import { DocumentNav } from './components/documents/DocumentNav'
import { DocumentWorkspace } from './components/documents/DocumentWorkspace'
import { useApiStatus } from './hooks/useApiStatus'
import { useWorkspaceData } from './hooks/useWorkspaceData'
import type {
  AiProviderSelection,
  DocumentItem,
  DocumentReviewWorkflowResponse,
  Message,
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
  const apiStatus = useApiStatus()
  const workspace = useWorkspaceData()
  const [messages, setMessages] = useState<Message[]>([])
  const [uploadedDocuments, setUploadedDocuments] = useState<DocumentItem[]>([])
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null)
  const [uploadState, setUploadState] = useState<'idle' | 'uploading' | 'failed'>('idle')
  const [workflowState, setWorkflowState] = useState<'idle' | 'running' | 'failed'>('idle')
  const [workflowResult, setWorkflowResult] =
    useState<DocumentReviewWorkflowResponse | null>(null)
  const [isSendingMessage, setIsSendingMessage] = useState(false)
  const [aiProvider, setAiProvider] =
    useState<AiProviderSelection>(getStoredAiProvider)

  useEffect(() => {
    if (workspace.data !== null) {
      setMessages(workspace.data.messages)
    }
  }, [workspace.data])

  useEffect(() => {
    localStorage.setItem(aiProviderStorageKey, aiProvider)
  }, [aiProvider])

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

  const { citations, documents, toolResult } = workspace.data
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

      await streamChatMessage(
        {
          message,
          documentId: selectedDocument.id,
          history: nextMessages,
          aiProvider,
        },
        (chunk) => {
          setMessages((currentMessages) =>
            currentMessages.map((currentMessage) =>
              currentMessage.id === assistantMessageId
                ? {
                    ...currentMessage,
                    content: `${currentMessage.content}${chunk}`,
                  }
                : currentMessage,
            ),
          )
        },
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
      })

      setWorkflowResult(result)
      setWorkflowState('idle')
    } catch {
      setWorkflowState('failed')
    }
  }

  return (
    <main className="grid h-screen overflow-hidden bg-slate-100 text-slate-900 lg:grid-cols-[272px_minmax(0,1fr)] xl:grid-cols-[288px_minmax(0,1fr)_500px] 2xl:grid-cols-[300px_minmax(0,1fr)_540px]">
      <DocumentNav
        documents={visibleDocuments}
        onSelectDocument={setSelectedDocumentId}
        onUploadDocument={handleUploadDocument}
        selectedDocumentId={selectedDocument.id}
        uploadState={uploadState}
      />
      <DocumentWorkspace
        citations={citations}
        document={selectedDocument}
        onRunWorkflow={handleRunWorkflow}
        toolResult={toolResult}
        workflowResult={workflowResult}
        workflowState={workflowState}
      />
      <AssistantPanel
        apiState={apiStatus.state}
        apiStatus={apiStatus.status}
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
