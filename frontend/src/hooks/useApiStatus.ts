import { useEffect, useState } from 'react'
import { getApiStatus } from '../api/statusApi'
import type { ApiConnectionState, ApiStatusResponse } from '../types'

type ApiStatusResult = {
  state: ApiConnectionState
  status: ApiStatusResponse | null
}

export function useApiStatus(): ApiStatusResult {
  const [result, setResult] = useState<ApiStatusResult>({
    state: 'loading',
    status: null,
  })

  useEffect(() => {
    const abortController = new AbortController()

    getApiStatus(abortController.signal)
      .then((status) => {
        setResult({
          state: 'connected',
          status,
        })
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setResult({
          state: 'unavailable',
          status: null,
        })
      })

    return () => {
      abortController.abort()
    }
  }, [])

  return result
}
