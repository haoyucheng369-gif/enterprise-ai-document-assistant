import { useEffect, useState } from 'react'
import { streamChatMessage } from './api/chatApi'
import { AssistantPanel } from './components/assistant/AssistantPanel'
import { DocumentNav } from './components/documents/DocumentNav'
import { DocumentWorkspace } from './components/documents/DocumentWorkspace'
import { useApiStatus } from './hooks/useApiStatus'
import { useWorkspaceData } from './hooks/useWorkspaceData'
import type { Message } from './types'

function App() {
  const apiStatus = useApiStatus()
  const workspace = useWorkspaceData()
  const [messages, setMessages] = useState<Message[]>([])
  const [isSendingMessage, setIsSendingMessage] = useState(false)

  useEffect(() => {
    if (workspace.data !== null) {
      setMessages(workspace.data.messages)
    }
  }, [workspace.data])

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
  const selectedDocument = documents[0]

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

  return (
    <main className="grid h-screen overflow-hidden bg-slate-100 text-slate-900 lg:grid-cols-[272px_minmax(0,1fr)] xl:grid-cols-[288px_minmax(0,1fr)_500px] 2xl:grid-cols-[300px_minmax(0,1fr)_540px]">
      <DocumentNav documents={documents} selectedDocumentId={selectedDocument.id} />
      <DocumentWorkspace
        citations={citations}
        document={selectedDocument}
        toolResult={toolResult}
      />
      <AssistantPanel
        apiState={apiStatus.state}
        apiStatus={apiStatus.status}
        isSending={isSendingMessage}
        messages={messages}
        onSendMessage={handleSendMessage}
      />
    </main>
  )
}

export default App
