import type { UserIdentity, WorkspaceResponse } from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function getWorkspaceData(
  userId: UserIdentity,
  signal?: AbortSignal,
): Promise<WorkspaceResponse> {
  const response = await fetch(`${apiBaseUrl}/api/workspace`, {
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
    }),
    signal,
  })

  await ensureSuccess(response, 'Workspace request')

  return response.json() as Promise<WorkspaceResponse>
}
