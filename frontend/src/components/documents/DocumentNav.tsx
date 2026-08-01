import type { DocumentItem, UserIdentity } from '../../types'
import { DocumentList } from './DocumentList'
import { DocumentUploadPanel } from './DocumentUploadPanel'

type DocumentNavProps = {
  documents: DocumentItem[]
  selectedDocumentId: string
  uploadState: 'idle' | 'uploading' | 'failed'
  deletingDocumentId?: string | null
  documentActionError?: string | null
  currentUserId: UserIdentity
  onSelectDocument: (documentId: string) => void
  onDeleteDocument: (documentId: string) => Promise<void>
  onUploadDocument: (file: File, allowedUserIds: UserIdentity[]) => Promise<void>
}

export function DocumentNav({
  documents,
  currentUserId,
  deletingDocumentId,
  documentActionError,
  onDeleteDocument,
  onSelectDocument,
  onUploadDocument,
  selectedDocumentId,
  uploadState,
}: DocumentNavProps) {
  return (
    <aside
      aria-label="Documents"
      className="grid min-h-0 grid-rows-[auto_minmax(0,1fr)] overflow-hidden border-b border-slate-200 bg-white p-4 lg:border-b-0 lg:border-r"
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

      <div className="mt-6 flex min-h-0 flex-col">
        <span className="text-sm font-semibold text-slate-700">Documents</span>
        <DocumentUploadPanel
          currentUserId={currentUserId}
          onUploadDocument={onUploadDocument}
          uploadState={uploadState}
        />

        {documentActionError ? (
          <p className="mt-2 rounded-md border border-rose-200 bg-rose-50 px-2.5 py-2 text-[11px] text-rose-700">
            {documentActionError}
          </p>
        ) : null}

        <DocumentList
          currentUserId={currentUserId}
          deletingDocumentId={deletingDocumentId}
          documents={documents}
          onDeleteDocument={onDeleteDocument}
          onSelectDocument={onSelectDocument}
          selectedDocumentId={selectedDocumentId}
        />
      </div>
    </aside>
  )
}
