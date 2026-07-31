import type { ApiStatusResponse } from '../types'
import { apiBaseUrl, ensureSuccess } from './apiClient'

export async function getApiStatus(
  signal?: AbortSignal,
): Promise<ApiStatusResponse> {
  const response = await fetch(`${apiBaseUrl}/api/status`, {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  await ensureSuccess(response, 'Status request')

  return response.json() as Promise<ApiStatusResponse>
}
