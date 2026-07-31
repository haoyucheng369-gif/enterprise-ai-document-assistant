import type {
  AiProviderSelection,
  DocumentUploadResponse,
  UserIdentity,
} from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function uploadDocument(
  file: File,
  aiProvider: AiProviderSelection,
  userId: UserIdentity,
  allowedUserIds: UserIdentity[],
): Promise<DocumentUploadResponse> {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('aiProvider', aiProvider)
  formData.append('allowedUserIds', allowedUserIds.join(','))

  const response = await fetch(`${apiBaseUrl}/api/documents/upload`, {
    method: 'POST',
    headers: buildUserHeaders(userId),
    body: formData,
  })

  await ensureSuccess(response, 'Document upload')

  return response.json() as Promise<DocumentUploadResponse>
}

export async function deleteDocument(
  documentId: string,
  userId: UserIdentity,
): Promise<void> {
  const response = await fetch(`${apiBaseUrl}/api/documents/${documentId}`, {
    method: 'DELETE',
    headers: buildUserHeaders(userId),
  })

  await ensureSuccess(response, 'Document delete')
}
