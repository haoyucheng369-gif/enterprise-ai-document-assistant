import type {
  ClassificationSkillRequest,
  ClassificationSkillResponse,
} from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function classifyDocument(
  request: ClassificationSkillRequest,
): Promise<ClassificationSkillResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/skills/classification`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Document classification failed with ${response.status}`)
  }

  return response.json() as Promise<ClassificationSkillResponse>
}
