import { MessageSquare, Send } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import type { ApiConnectionState, ApiStatusResponse, Message } from '../../types'
import { ApiStatusBadge } from '../layout/ApiStatusBadge'

type AssistantPanelProps = {
  messages: Message[]
  apiState: ApiConnectionState
  apiStatus: ApiStatusResponse | null
  isSending: boolean
  onSendMessage: (message: string) => Promise<void>
}

export function AssistantPanel({
  messages,
  apiState,
  apiStatus,
  isSending,
  onSendMessage,
}: AssistantPanelProps) {
  const [draftMessage, setDraftMessage] = useState('')
  const messageListRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const messageList = messageListRef.current
    if (messageList === null) {
      return
    }

    messageList.scrollTo({
      top: messageList.scrollHeight,
      behavior: 'smooth',
    })
  }, [messages, isSending])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const message = draftMessage.trim()
    if (message.length === 0 || isSending) {
      return
    }

    setDraftMessage('')
    await onSendMessage(message)
  }

  return (
    <aside
      aria-label="AI Assistant"
      className="min-h-0 border-t border-slate-200 bg-white p-4 lg:col-span-2 xl:col-span-1 xl:border-l xl:border-t-0"
    >
      <div className="grid h-full min-h-0 grid-rows-[auto_minmax(0,1fr)_auto] gap-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="inline-flex items-center gap-2 text-base font-semibold text-slate-950">
              <MessageSquare className="text-indigo-600" size={17} />
              Assistant
            </h2>
            <p className="mt-0.5 text-xs text-slate-500">Document context</p>
          </div>
          <ApiStatusBadge state={apiState} status={apiStatus} />
        </div>

        <div
          className="flex min-h-0 flex-col gap-2 overflow-y-auto pr-1"
          ref={messageListRef}
        >
          {messages.map((message) => (
            <div
              className={`rounded-md border p-3 ${
                message.role === 'assistant'
                  ? 'border-indigo-100 bg-indigo-50/50'
                  : 'border-blue-100 bg-blue-50'
              }`}
              key={message.id}
            >
              <span className="text-xs text-slate-500">
                {message.role === 'assistant' ? 'Assistant' : 'You'}
              </span>
              <p className="mt-1 text-sm leading-6 text-slate-700">
                {message.content}
              </p>
            </div>
          ))}
        </div>

        <form
          className="flex gap-2 rounded-md border border-slate-300 bg-white p-2"
          onSubmit={handleSubmit}
        >
          <input
            aria-label="Message"
            className="min-w-0 flex-1 border-0 bg-transparent text-slate-900 outline-none placeholder:text-slate-400"
            disabled={isSending}
            onChange={(event) => setDraftMessage(event.target.value)}
            placeholder="Ask about this document..."
            type="text"
            value={draftMessage}
          />
          <button
            aria-label="Send message"
            className="inline-flex min-h-9 cursor-pointer items-center gap-2 rounded-md bg-blue-600 px-3 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-slate-300"
            disabled={isSending}
            type="submit"
          >
            <Send size={16} />
            {isSending ? 'Sending' : 'Send'}
          </button>
        </form>
      </div>
    </aside>
  )
}
