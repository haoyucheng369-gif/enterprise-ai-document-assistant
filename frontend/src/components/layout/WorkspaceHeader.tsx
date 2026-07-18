import { Workflow } from 'lucide-react'
import type { DocumentItem } from '../../types'

type WorkspaceHeaderProps = {
  document: DocumentItem
  isWorkflowRunning: boolean
  onRunWorkflow: () => Promise<void>
}

export function WorkspaceHeader({
  document,
  isWorkflowRunning,
  onRunWorkflow,
}: WorkspaceHeaderProps) {
  return (
    <header className="flex min-h-14 flex-col justify-between gap-3 sm:flex-row sm:items-center">
      <div>
        <p className="text-xs font-medium uppercase tracking-[0.08em] text-blue-600">
          {document.type}
        </p>
        <h2 className="text-xl font-semibold text-slate-950">{document.title}</h2>
      </div>
      <div className="flex gap-2">
        <button
          className="inline-flex min-h-9 cursor-pointer items-center gap-2 rounded-md border border-indigo-200 bg-indigo-50 px-3 text-sm font-medium text-indigo-800 transition hover:bg-indigo-100 disabled:cursor-wait disabled:opacity-70"
          disabled={isWorkflowRunning}
          onClick={onRunWorkflow}
          type="button"
        >
          <Workflow size={16} />
          {isWorkflowRunning ? 'Running workflow' : 'Run workflow'}
        </button>
      </div>
    </header>
  )
}
