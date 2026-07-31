import type {
  AiProviderSelection,
  DocumentUploadResponse,
  UserIdentity,
} from '../types'
import { buildUserHeaders } from './requestHeaders'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function uploadDocument(
  file: File,
  aiProvider: AiProviderSelection,
  userId: UserIdentity,
  allowedUserIds: UserIdentity[],
): Promise<DocumentUploadResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const formData = new FormData()
  formData.append('file', file)
  formData.append('aiProvider', aiProvider)
  formData.append('allowedUserIds', allowedUserIds.join(','))

  const response = await fetch(`${apiBaseUrl}/api/documents/upload`, {
    method: 'POST',
    headers: buildUserHeaders(userId),
    body: formData,
  })

  if (!response.ok) {
    throw new Error(`Document upload failed with ${response.status}`)
  }

  return response.json() as Promise<DocumentUploadResponse>
}

export async function deleteDocument(
  documentId: string,
  userId: UserIdentity,
): Promise<void> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl

  const response = await fetch(`${apiBaseUrl}/api/documents/${documentId}`, {
    method: 'DELETE',
    headers: buildUserHeaders(userId),
  })

  if (!response.ok) {
    throw new Error(`Document delete failed with ${response.status}`)
  }
}
