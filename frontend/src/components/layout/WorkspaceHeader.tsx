import type { DocumentItem } from '../../types'

type WorkspaceHeaderProps = {
  document: DocumentItem
}

export function WorkspaceHeader({
  document,
}: WorkspaceHeaderProps) {
  return (
    <header className="grid gap-1">
      <div className="min-w-0">
        <p className="text-xs font-medium uppercase tracking-[0.08em] text-blue-600">
          Evidence
        </p>
        <h2 className="truncate text-lg font-semibold text-slate-950">
          {document.title}
        </h2>
      </div>
    </header>
  )
}
