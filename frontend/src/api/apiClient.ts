import type { UserIdentity } from '../types'

export const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5221'

export function buildUserHeaders(
  userId: UserIdentity,
  headers: Record<string, string> = {},
) {
  return {
    ...headers,
    'X-User-Id': userId,
  }
}

export async function ensureSuccess(
  response: Response,
  operation: string,
): Promise<void> {
  if (response.ok) {
    return
  }

  // Prefer the API's ProblemDetails message while retaining a useful fallback.
  let problemMessage = ''

  try {
    const problem = (await response.clone().json()) as {
      detail?: string
      title?: string
    }
    problemMessage = problem.detail ?? problem.title ?? ''
  } catch {
    // Some infrastructure errors do not return JSON.
  }

  throw new Error(
    problemMessage || `${operation} failed with ${response.status}`,
  )
}
