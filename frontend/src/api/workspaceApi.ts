import type { UserIdentity, WorkspaceResponse } from '../types'
import { buildUserHeaders } from './requestHeaders'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function getWorkspaceData(
  userId: UserIdentity,
  signal?: AbortSignal,
): Promise<WorkspaceResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/workspace`, {
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
    }),
    signal,
  })

  if (!response.ok) {
    throw new Error(`Workspace request failed with ${response.status}`)
  }

  return response.json() as Promise<WorkspaceResponse>
}
