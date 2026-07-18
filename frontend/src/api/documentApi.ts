import type { DocumentUploadResponse } from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function uploadDocument(file: File): Promise<DocumentUploadResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const formData = new FormData()
  formData.append('file', file)

  const response = await fetch(`${apiBaseUrl}/api/documents/upload`, {
    method: 'POST',
    body: formData,
  })

  if (!response.ok) {
    throw new Error(`Document upload failed with ${response.status}`)
  }

  return response.json() as Promise<DocumentUploadResponse>
}
