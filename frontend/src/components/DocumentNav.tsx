import { FileText, Upload } from 'lucide-react'
import type { DocumentItem } from '../types'

type DocumentNavProps = {
  documents: DocumentItem[]
  selectedDocumentId: string
}

export function DocumentNav({
  documents,
  selectedDocumentId,
}: DocumentNavProps) {
  return (
    <aside
      aria-label="Documents"
      className="border-b border-slate-200 bg-white p-4 lg:border-b-0 lg:border-r"
    >
      <div className="grid grid-cols-[34px_minmax(0,1fr)] items-center gap-3">
        <div
          aria-hidden="true"
          className="grid size-[34px] place-items-center rounded-md border border-indigo-200 bg-indigo-50 text-xs font-semibold text-indigo-700"
        >
          ED
        </div>
        <div>
          <h1 className="text-sm font-semibold leading-tight text-slate-950">
            Document Assistant
          </h1>
          <p className="text-xs text-slate-500">Workspace</p>
        </div>
      </div>

      <div className="mt-6 flex items-center justify-between">
        <span className="text-sm font-semibold text-slate-700">Documents</span>
        <button
          aria-label="Upload document"
          className="inline-flex size-8 cursor-pointer items-center justify-center rounded-md border border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100"
          type="button"
        >
          <Upload size={15} />
        </button>
      </div>

      <div className="mt-3 grid gap-2">
        {documents.map((document) => (
          <button
            className={`grid cursor-pointer gap-1 rounded-md border p-3 text-left transition ${
              document.id === selectedDocumentId
                ? 'border-blue-300 bg-blue-50'
                : 'border-slate-200 bg-white hover:bg-slate-50'
            }`}
            key={document.id}
            type="button"
          >
            <span className="flex items-center gap-2 text-sm font-semibold text-slate-900">
              <FileText
                className={
                  document.id === selectedDocumentId
                    ? 'text-blue-600'
                    : 'text-slate-400'
                }
                size={15}
              />
              {document.title}
            </span>
            <span className="text-xs text-slate-500">
              {document.type} - {document.updatedAt}
            </span>
            <span
              className={`mt-1 w-fit rounded-sm px-2 py-0.5 text-[11px] font-medium ${
                document.status === 'Queued'
                  ? 'bg-amber-50 text-amber-700'
                  : 'bg-emerald-50 text-emerald-700'
              }`}
            >
              {document.status}
            </span>
          </button>
        ))}
      </div>
    </aside>
  )
}
