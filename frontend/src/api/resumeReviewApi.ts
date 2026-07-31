import type {
  ResumeReviewSkillRequest,
  ResumeReviewSkillResponse,
  UserIdentity,
} from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function generateResumeReview(
  request: ResumeReviewSkillRequest,
  userId: UserIdentity,
): Promise<ResumeReviewSkillResponse> {
  const response = await fetch(`${apiBaseUrl}/api/skills/resume-review`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
  })

  await ensureSuccess(response, 'Resume review')

  return response.json() as Promise<ResumeReviewSkillResponse>
}
