import type { UserIdentity } from '../types'

export function buildUserHeaders(
  userId: UserIdentity,
  headers: Record<string, string> = {},
) {
  return {
    ...headers,
    'X-User-Id': userId,
  }
}
