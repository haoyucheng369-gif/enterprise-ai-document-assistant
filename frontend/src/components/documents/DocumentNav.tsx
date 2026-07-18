import { FileText, Upload } from 'lucide-react'
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
      className="min-h-0 overflow-y-auto border-b border-slate-200 bg-white p-4 lg:border-b-0 lg:border-r"
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
          {uploadState === 'uploading' ? 'Uploading document' : 'Drop file or browse'}
        </span>
        <span
          className={`text-[11px] ${
            uploadState === 'failed' ? 'text-rose-600' : 'text-slate-400'
          }`}
        >
          {uploadState === 'failed' ? 'Upload failed' : 'TXT, MD, PDF, DOCX'}
        </span>
      </button>

      <div className="mt-3 grid gap-2">
        {documents.map((document) => (
          <button
            className={`grid cursor-pointer gap-1 rounded-md border p-3 text-left transition ${
              document.id === selectedDocumentId
                ? 'border-blue-300 bg-blue-50'
                : 'border-slate-200 bg-white hover:bg-slate-50'
            }`}
            key={document.id}
            onClick={() => onSelectDocument(document.id)}
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
