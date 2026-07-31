import type {
  DocumentReviewWorkflowRequest,
  DocumentReviewWorkflowResponse,
  UserIdentity,
} from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function runDocumentReviewWorkflow(
  request: DocumentReviewWorkflowRequest,
  userId: UserIdentity,
): Promise<DocumentReviewWorkflowResponse> {
  const response = await fetch(`${apiBaseUrl}/api/workflows/document-review`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
  })

  await ensureSuccess(response, 'Document review workflow')

  return response.json() as Promise<DocumentReviewWorkflowResponse>
}
