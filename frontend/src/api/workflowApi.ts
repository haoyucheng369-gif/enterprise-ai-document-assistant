import type {
  DocumentReviewWorkflowRequest,
  DocumentReviewWorkflowResponse,
  UserIdentity,
} from '../types'
import { buildUserHeaders } from './requestHeaders'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function runDocumentReviewWorkflow(
  request: DocumentReviewWorkflowRequest,
  userId: UserIdentity,
): Promise<DocumentReviewWorkflowResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/workflows/document-review`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Document review workflow failed with ${response.status}`)
  }

  return response.json() as Promise<DocumentReviewWorkflowResponse>
}
