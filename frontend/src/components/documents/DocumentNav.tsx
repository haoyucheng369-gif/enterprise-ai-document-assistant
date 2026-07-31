import {
  File,
  FileCode,
  FileText,
  FileType,
  Trash2,
  type LucideIcon,
  Upload,
} from 'lucide-react'
import { type ChangeEvent, type DragEvent, useEffect, useRef, useState } from 'react'
import type { DocumentItem, UserIdentity } from '../../types'

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
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDraggingFile, setIsDraggingFile] = useState(false)
  const [allowedUserIds, setAllowedUserIds] = useState<UserIdentity[]>([])

  useEffect(() => {
    setAllowedUserIds((currentIds) =>
      currentIds.filter((userId) => userId !== currentUserId),
    )
  }, [currentUserId])

  async function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''

    if (file) {
      await onUploadDocument(file, allowedUserIds)
    }
  }

  async function handleDrop(event: DragEvent<HTMLButtonElement>) {
    event.preventDefault()
    setIsDraggingFile(false)

    const file = event.dataTransfer.files[0]
    if (file) {
      await onUploadDocument(file, allowedUserIds)
    }
  }

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
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold text-slate-700">Documents</span>
          <input
            ref={inputRef}
            accept=".txt,.md,.pdf,.docx"
            className="hidden"
            onChange={handleFileChange}
            type="file"
          />
        </div>

        <button
          aria-label="Upload document"
          className={`mt-3 grid w-full cursor-pointer place-items-center gap-2 rounded-md border border-dashed px-3 py-4 text-center transition ${
            isDraggingFile
              ? 'border-blue-400 bg-blue-50 text-blue-700'
              : 'border-slate-300 bg-slate-50 text-slate-600 hover:border-blue-300 hover:bg-blue-50 hover:text-blue-700'
          } ${uploadState === 'uploading' ? 'cursor-wait opacity-75' : ''}`}
          disabled={uploadState === 'uploading'}
          onClick={() => inputRef.current?.click()}
          onDragEnter={(event) => {
            event.preventDefault()
            setIsDraggingFile(true)
          }}
          onDragLeave={(event) => {
            event.preventDefault()
            setIsDraggingFile(false)
          }}
          onDragOver={(event) => event.preventDefault()}
          onDrop={handleDrop}
          type="button"
        >
          <span className="grid size-8 place-items-center rounded-md bg-white text-blue-600 shadow-sm ring-1 ring-slate-200">
            <Upload size={16} />
          </span>
          <span className="text-xs font-medium">
            {uploadState === 'uploading'
              ? 'Uploading document'
              : 'Drop file or browse'}
          </span>
          <span
            className={`text-[11px] ${
              uploadState === 'failed' ? 'text-rose-600' : 'text-slate-400'
            }`}
          >
            {uploadState === 'failed' ? 'Upload failed' : 'TXT, MD, PDF, DOCX'}
          </span>
        </button>

        <fieldset className="mt-2 rounded-md border border-slate-200 bg-slate-50 px-2.5 py-2">
          <legend className="px-1 text-[11px] font-medium text-slate-500">
            Allow readers
          </legend>
          <div className="flex flex-wrap gap-1.5">
            {userIdentities
              .filter((userId) => userId !== currentUserId)
              .map((userId) => {
                const isSelected = allowedUserIds.includes(userId)

                return (
                  <label
                    className={`cursor-pointer rounded-sm border px-2 py-1 text-[11px] font-medium capitalize transition ${
                      isSelected
                        ? 'border-emerald-300 bg-emerald-50 text-emerald-700'
                        : 'border-slate-200 bg-white text-slate-500 hover:border-slate-300'
                    }`}
                    key={userId}
                  >
                    <input
                      checked={isSelected}
                      className="sr-only"
                      onChange={() => {
                        setAllowedUserIds((currentIds) =>
                          isSelected
                            ? currentIds.filter((id) => id !== userId)
                            : [...currentIds, userId],
                        )
                      }}
                      type="checkbox"
                    />
                    {userId}
                  </label>
                )
              })}
          </div>
        </fieldset>

        {documentActionError ? (
          <p className="mt-2 rounded-md border border-rose-200 bg-rose-50 px-2.5 py-2 text-[11px] text-rose-700">
            {documentActionError}
          </p>
        ) : null}

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
      </div>
    </aside>
  )
}

const userIdentities: UserIdentity[] = ['local-user', 'alice', 'bob', 'charlie']

function getDocumentIcon(type: string): {
  icon: LucideIcon
  color: string
  background: string
} {
  switch (type.toUpperCase()) {
    case 'PDF':
      return {
        icon: FileText,
        color: 'text-rose-600',
        background: 'bg-rose-50',
      }
    case 'DOCX':
      return {
        icon: FileType,
        color: 'text-blue-600',
        background: 'bg-blue-50',
      }
    case 'MD':
      return {
        icon: FileCode,
        color: 'text-violet-600',
        background: 'bg-violet-50',
      }
    case 'TXT':
      return {
        icon: FileText,
        color: 'text-slate-600',
        background: 'bg-slate-100',
      }
    default:
      return {
        icon: File,
        color: 'text-emerald-600',
        background: 'bg-emerald-50',
      }
  }
}
