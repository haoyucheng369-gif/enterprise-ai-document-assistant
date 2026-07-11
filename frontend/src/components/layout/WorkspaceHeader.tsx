import { FileSearch, ListChecks } from 'lucide-react'
import type { DocumentItem } from '../../types'

type WorkspaceHeaderProps = {
  document: DocumentItem
}

export function WorkspaceHeader({ document }: WorkspaceHeaderProps) {
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
          className="inline-flex min-h-9 cursor-pointer items-center gap-2 rounded-md border border-emerald-200 bg-emerald-50 px-3 text-sm text-emerald-800 hover:bg-emerald-100"
          type="button"
        >
          <ListChecks size={16} />
          Summarize
        </button>
        <button
          className="inline-flex min-h-9 cursor-pointer items-center gap-2 rounded-md border border-amber-200 bg-amber-50 px-3 text-sm text-amber-800 hover:bg-amber-100"
          type="button"
        >
          <FileSearch size={16} />
          Analyze risks
        </button>
      </div>
    </header>
  )
}
