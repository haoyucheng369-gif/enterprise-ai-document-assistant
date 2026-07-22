import {
  File,
  FileCode,
  FileText,
  FileType,
  type LucideIcon,
  Upload,
} from 'lucide-react'
import { type ChangeEvent, type DragEvent, useRef, useState } from 'react'
import type { DocumentItem } from '../../types'

type DocumentNavProps = {
  documents: DocumentItem[]
  selectedDocumentId: string
  uploadState: 'idle' | 'uploading' | 'failed'
  onSelectDocument: (documentId: string) => void
  onUploadDocument: (file: File) => Promise<void>
}

export function DocumentNav({
  documents,
  onSelectDocument,
  onUploadDocument,
  selectedDocumentId,
  uploadState,
}: DocumentNavProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDraggingFile, setIsDraggingFile] = useState(false)

  async function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''

    if (file) {
      await onUploadDocument(file)
    }
  }

  async function handleDrop(event: DragEvent<HTMLButtonElement>) {
    event.preventDefault()
    setIsDraggingFile(false)

    const file = event.dataTransfer.files[0]
    if (file) {
      await onUploadDocument(file)
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

        <div className="mt-3 grid min-h-0 gap-2 overflow-y-auto pr-1">
          {documents.map((document) => {
            const fileIcon = getDocumentIcon(document.type)
            const Icon = fileIcon.icon

            return (
              <button
                className={`grid cursor-pointer gap-1.5 rounded-md border px-3 py-2.5 text-left transition ${
                  document.id === selectedDocumentId
                    ? 'border-blue-300 bg-blue-50'
                    : 'border-slate-200 bg-white hover:bg-slate-50'
                }`}
                key={document.id}
                onClick={() => onSelectDocument(document.id)}
                type="button"
              >
                <span className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                  <span
                    className={`grid size-5 shrink-0 place-items-center rounded-sm ${fileIcon.background} ${fileIcon.color}`}
                  >
                    <Icon size={14} />
                  </span>
                  {document.title}
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
            )
          })}
        </div>
      </div>
    </aside>
  )
}

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
