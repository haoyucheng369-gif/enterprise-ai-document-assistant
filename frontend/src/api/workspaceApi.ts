import type { WorkspaceResponse } from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function getWorkspaceData(
  signal?: AbortSignal,
): Promise<WorkspaceResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/workspace`, {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Workspace request failed with ${response.status}`)
  }

  return response.json() as Promise<WorkspaceResponse>
}
