import { Upload } from 'lucide-react'
import { type ChangeEvent, type DragEvent, useEffect, useRef, useState } from 'react'
import type { UserIdentity } from '../../types'

type DocumentUploadPanelProps = {
  currentUserId: UserIdentity
  uploadState: 'idle' | 'uploading' | 'failed'
  onUploadDocument: (file: File, allowedUserIds: UserIdentity[]) => Promise<void>
}

const userIdentities: UserIdentity[] = ['local-user', 'alice', 'bob', 'charlie']

export function DocumentUploadPanel({
  currentUserId,
  onUploadDocument,
  uploadState,
}: DocumentUploadPanelProps) {
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
    <>
      <input
        ref={inputRef}
        accept=".txt,.md,.pdf,.docx"
        className="hidden"
        onChange={handleFileChange}
        type="file"
      />
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
    </>
  )
}
