import type {
  ClassificationSkillRequest,
  ClassificationSkillResponse,
  UserIdentity,
} from '../types'
import { buildUserHeaders } from './requestHeaders'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function classifyDocument(
  request: ClassificationSkillRequest,
  userId: UserIdentity,
): Promise<ClassificationSkillResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/skills/classification`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Document classification failed with ${response.status}`)
  }

  return response.json() as Promise<ClassificationSkillResponse>
}
