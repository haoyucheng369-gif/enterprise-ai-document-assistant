import type { AiProviderSelection, DocumentUploadResponse } from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function uploadDocument(
  file: File,
  aiProvider: AiProviderSelection,
): Promise<DocumentUploadResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const formData = new FormData()
  formData.append('file', file)
  formData.append('aiProvider', aiProvider)

  const response = await fetch(`${apiBaseUrl}/api/documents/upload`, {
    method: 'POST',
    body: formData,
  })

  if (!response.ok) {
    throw new Error(`Document upload failed with ${response.status}`)
  }

  return response.json() as Promise<DocumentUploadResponse>
}

export async function deleteDocument(documentId: string): Promise<void> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl

  const response = await fetch(`${apiBaseUrl}/api/documents/${documentId}`, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error(`Document delete failed with ${response.status}`)
  }
}
