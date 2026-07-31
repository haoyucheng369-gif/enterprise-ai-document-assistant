import type {
  ClassificationSkillRequest,
  ClassificationSkillResponse,
  UserIdentity,
} from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function classifyDocument(
  request: ClassificationSkillRequest,
  userId: UserIdentity,
): Promise<ClassificationSkillResponse> {
  const response = await fetch(`${apiBaseUrl}/api/skills/classification`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
  })

  await ensureSuccess(response, 'Document classification')

  return response.json() as Promise<ClassificationSkillResponse>
}
