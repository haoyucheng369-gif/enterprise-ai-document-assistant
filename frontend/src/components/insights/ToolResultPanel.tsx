import { Activity } from 'lucide-react'
import type { ToolResult } from '../../types'

type ToolResultPanelProps = {
  toolResult: ToolResult
}

export function ToolResultPanel({ toolResult }: ToolResultPanelProps) {
  return (
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-center justify-between">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
          <Activity className="text-violet-600" size={16} />
          Tool results
        </h3>
        <span className="rounded-sm bg-violet-50 px-2 py-0.5 text-[11px] font-medium text-violet-700">
          {toolResult.status}
        </span>
      </div>
      <div className="mt-4 rounded-md border border-violet-100 bg-violet-50/50 p-3 text-sm leading-6 text-slate-700">
        <span className="block font-semibold text-slate-900">
          {toolResult.name}
        </span>
        <p className="mt-1">{toolResult.description}</p>
      </div>
    </section>
  )
}
