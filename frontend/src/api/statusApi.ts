import type { ApiStatusResponse } from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

export async function getApiStatus(
  signal?: AbortSignal,
): Promise<ApiStatusResponse> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
  const response = await fetch(`${apiBaseUrl}/api/status`, {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Status request failed with ${response.status}`)
  }

  return response.json() as Promise<ApiStatusResponse>
}
