import { MessageSquare, Send } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import type {
  AiProviderSelection,
  Message,
} from '../../types'
import { AiProviderSelector } from './AiProviderSelector'

type AssistantPanelProps = {
  messages: Message[]
  aiProvider: AiProviderSelection
  isSending: boolean
  onSelectAiProvider: (provider: AiProviderSelection) => void
  onSendMessage: (message: string) => Promise<void>
}

export function AssistantPanel({
  messages,
  aiProvider,
  isSending,
  onSelectAiProvider,
  onSendMessage,
}: AssistantPanelProps) {
  const [draftMessage, setDraftMessage] = useState('')
  const messageListRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const shouldRestoreFocusRef = useRef(false)

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

  useEffect(() => {
    if (isSending || !shouldRestoreFocusRef.current) {
      return
    }

    inputRef.current?.focus()
    shouldRestoreFocusRef.current = false
  }, [isSending])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const message = draftMessage.trim()
    if (message.length === 0 || isSending) {
      return
    }

    setDraftMessage('')
    shouldRestoreFocusRef.current = true
    await onSendMessage(message)
  }

  async function handleSuggestedAction(action: string) {
    if (isSending) {
      return
    }

    shouldRestoreFocusRef.current = true
    await onSendMessage(toUserAction(action))
  }

  return (
    <aside
      aria-label="AI Assistant"
      className="min-h-0 overflow-hidden border-t border-slate-200 bg-white px-6 py-4 lg:col-span-2 xl:col-span-1 xl:border-l xl:border-t-0"
    >
      <div className="grid h-full min-h-0 grid-rows-[auto_minmax(0,1fr)] gap-3">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="min-w-0">
            <h2 className="inline-flex items-center gap-2 text-base font-semibold text-slate-950">
              <MessageSquare className="text-indigo-600" size={17} />
              Assistant
            </h2>
            <p className="mt-0.5 text-xs text-slate-500">Document context</p>
          </div>
          <AiProviderSelector
            onSelectProvider={onSelectAiProvider}
            selectedProvider={aiProvider}
          />
        </div>

        <div className="grid min-h-0 overflow-hidden">
          <section className="grid min-h-0 grid-rows-[auto_minmax(0,1fr)_auto] rounded-md border border-slate-200 bg-slate-50">
            <div className="border-b border-slate-200 px-3 py-2">
              <h3 className="text-sm font-semibold text-slate-800">
                Conversation
              </h3>
            </div>

            <div
              className="flex min-h-0 flex-col gap-2 overflow-y-auto p-3"
              ref={messageListRef}
            >
              {messages.map((message) => (
                <div
                  className={`rounded-md border p-3 ${
                    message.role === 'assistant'
                      ? 'border-indigo-100 bg-white'
                      : 'border-blue-100 bg-blue-50'
                  }`}
                  key={message.id}
                >
                  <span className="text-xs text-slate-500">
                    {message.role === 'assistant' ? 'Assistant' : 'You'}
                  </span>
                  <p className="mt-1 text-sm leading-6 text-slate-700">
                    {message.content.length > 0 ? message.content : 'Thinking...'}
                  </p>
                  {message.role === 'assistant' && message.confidence !== undefined ? (
                    <p className="mt-2 text-xs font-medium text-slate-500">
                      Confidence: {message.confidence}
                    </p>
                  ) : null}
                  {message.role === 'assistant' &&
                  message.suggestedActions !== undefined &&
                  message.suggestedActions.length > 0 ? (
                    <div className="mt-3">
                      <p className="mb-1.5 text-xs text-slate-500">
                        Suggested next steps
                      </p>
                      <div className="flex flex-wrap gap-2">
                        {message.suggestedActions.map((action) => {
                          const userAction = toUserAction(action)

                          return (
                            <button
                              className="cursor-pointer rounded-md border border-indigo-200 bg-white px-2.5 py-1 text-left text-xs font-medium text-indigo-700 hover:border-indigo-300 hover:bg-indigo-50 disabled:cursor-not-allowed disabled:border-slate-200 disabled:bg-slate-50 disabled:text-slate-400"
                              disabled={isSending}
                              key={action}
                              onClick={() => void handleSuggestedAction(action)}
                              type="button"
                            >
                              {userAction}
                            </button>
                          )
                        })}
                      </div>
                    </div>
                  ) : null}
                </div>
              ))}
            </div>

            <form
              className="flex gap-2 border-t border-slate-200 bg-white p-2"
              onSubmit={handleSubmit}
            >
              <input
                aria-label="Message"
                className="min-w-0 flex-1 border-0 bg-transparent text-slate-900 outline-none placeholder:text-slate-400"
                disabled={isSending}
                onChange={(event) => setDraftMessage(event.target.value)}
                placeholder="Ask about this document..."
                ref={inputRef}
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
          </section>
        </div>
      </div>
    </aside>
  )
}

function toUserAction(action: string) {
  const normalized = action
    .trim()
    .replace(/^如果你愿意[，,]\s*/u, '')
    .replace(/^我可以继续帮你/u, '')
    .replace(/^我可以帮你/u, '')
    .replace(/^也可以帮你/u, '')
    .replace(/^要我帮你/u, '')
    .replace(/^需要我帮你/u, '')
    .replace(/^Would you like me to\s+/iu, '')
    .replace(/^Do you want me to\s+/iu, '')
    .replace(/^I can\s+/iu, '')
    .replace(/[。.]$/u, '')
    .replace(/吗[？?]$/u, '')
    .trim()

  return normalized.length > 0 ? normalized : action.trim()
}
