import { File, FileCode, FileText, FileType, Trash2, type LucideIcon } from 'lucide-react'
import type { DocumentItem, UserIdentity } from '../../types'

type DocumentListProps = {
  documents: DocumentItem[]
  selectedDocumentId: string
  deletingDocumentId?: string | null
  currentUserId: UserIdentity
  onSelectDocument: (documentId: string) => void
  onDeleteDocument: (documentId: string) => Promise<void>
}

export function DocumentList({
  currentUserId,
  deletingDocumentId,
  documents,
  onDeleteDocument,
  onSelectDocument,
  selectedDocumentId,
}: DocumentListProps) {
  return (
    <div className="mt-3 grid min-h-0 gap-2 overflow-y-auto pr-1">
      {documents.map((document) => {
        const fileIcon = getDocumentIcon(document.type)
        const Icon = fileIcon.icon

        return (
          <div
            className={`grid grid-cols-[minmax(0,1fr)_28px] items-center gap-2 rounded-md border px-3 py-2.5 transition ${
              document.id === selectedDocumentId
                ? 'border-blue-300 bg-blue-50'
                : 'border-slate-200 bg-white hover:bg-slate-50'
            }`}
            key={document.id}
          >
            <button
              className="grid min-w-0 cursor-pointer gap-1.5 text-left"
              onClick={() => onSelectDocument(document.id)}
              type="button"
            >
              <span className="flex min-w-0 items-center gap-2 text-sm font-semibold text-slate-900">
                <span
                  className={`grid size-5 shrink-0 place-items-center rounded-sm ${fileIcon.background} ${fileIcon.color}`}
                >
                  <Icon size={14} />
                </span>
                <span className="truncate">{document.title}</span>
              </span>
              <span className="flex items-center justify-between gap-2 text-xs text-slate-500">
                <span className="truncate">
                  {document.type} - {document.updatedAt}
                </span>
                <span
                  className={`shrink-0 rounded-sm px-1.5 py-0.5 text-[11px] font-medium ${
                    document.status === 'Queued'
                      ? 'bg-amber-50 text-amber-700'
                      : 'bg-emerald-50 text-emerald-700'
                  }`}
                >
                  {document.status}
                </span>
              </span>
            </button>
            {document.ownerId === currentUserId ? (
              <button
                aria-label={`Delete ${document.title}`}
                className="grid size-7 cursor-pointer place-items-center rounded-md text-slate-400 transition hover:bg-rose-50 hover:text-rose-600 disabled:cursor-wait disabled:opacity-50"
                disabled={deletingDocumentId === document.id}
                onClick={() => onDeleteDocument(document.id)}
                title="Delete document"
                type="button"
              >
                <Trash2 size={14} />
              </button>
            ) : null}
          </div>
        )
      })}
    </div>
  )
}

function getDocumentIcon(type: string): {
  icon: LucideIcon
  color: string
  background: string
} {
  switch (type.toUpperCase()) {
    case 'PDF':
      return { icon: FileText, color: 'text-rose-600', background: 'bg-rose-50' }
    case 'DOCX':
      return { icon: FileType, color: 'text-blue-600', background: 'bg-blue-50' }
    case 'MD':
      return { icon: FileCode, color: 'text-violet-600', background: 'bg-violet-50' }
    case 'TXT':
      return { icon: FileText, color: 'text-slate-600', background: 'bg-slate-100' }
    default:
      return { icon: File, color: 'text-emerald-600', background: 'bg-emerald-50' }
  }
}
