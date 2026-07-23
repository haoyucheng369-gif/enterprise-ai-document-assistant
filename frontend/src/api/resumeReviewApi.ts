import type {
  ResumeReviewSkillRequest,
  ResumeReviewSkillResponse,
} from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function generateResumeReview(
  request: ResumeReviewSkillRequest,
): Promise<ResumeReviewSkillResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/skills/resume-review`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Resume review failed with ${response.status}`)
  }

  return response.json() as Promise<ResumeReviewSkillResponse>
}
