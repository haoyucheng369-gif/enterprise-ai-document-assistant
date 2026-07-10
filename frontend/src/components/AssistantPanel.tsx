import { MessageSquare, Send } from 'lucide-react'
import type { Message } from '../types'

type AssistantPanelProps = {
  messages: Message[]
}

export function AssistantPanel({ messages }: AssistantPanelProps) {
  return (
    <aside
      aria-label="AI Assistant"
      className="grid min-h-[360px] border-t border-slate-200 bg-white p-4 lg:col-span-2 xl:col-span-1 xl:border-l xl:border-t-0"
    >
      <div className="grid grid-rows-[auto_minmax(0,1fr)_auto] gap-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="inline-flex items-center gap-2 text-base font-semibold text-slate-950">
              <MessageSquare className="text-indigo-600" size={17} />
              Assistant
            </h2>
            <p className="mt-0.5 text-xs text-slate-500">Document context</p>
          </div>
          <span className="rounded-sm bg-emerald-50 px-2 py-0.5 text-[11px] font-medium text-emerald-700">
            Online
          </span>
        </div>

        <div className="flex min-h-0 flex-col gap-2 overflow-auto">
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

        <form className="flex gap-2 rounded-md border border-slate-300 bg-white p-2">
          <input
            aria-label="Message"
            className="min-w-0 flex-1 border-0 bg-transparent text-slate-900 outline-none placeholder:text-slate-400"
            placeholder="Ask about this document..."
            type="text"
          />
          <button
            aria-label="Send message"
            className="inline-flex min-h-9 cursor-pointer items-center gap-2 rounded-md bg-blue-600 px-3 text-sm font-semibold text-white hover:bg-blue-700"
            type="submit"
          >
            <Send size={16} />
            Send
          </button>
        </form>
      </div>
    </aside>
  )
}
