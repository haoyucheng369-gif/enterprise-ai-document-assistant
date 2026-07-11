import { useEffect, useState } from 'react'
import { getWorkspaceData } from '../api/workspaceApi'
import type { DataConnectionState, WorkspaceResponse } from '../types'

type WorkspaceDataResult = {
  state: DataConnectionState
  data: WorkspaceResponse | null
}

export function useWorkspaceData(): WorkspaceDataResult {
  const [result, setResult] = useState<WorkspaceDataResult>({
    state: 'loading',
    data: null,
  })

  useEffect(() => {
    const abortController = new AbortController()

    getWorkspaceData(abortController.signal)
      .then((data) => {
        setResult({
          state: 'loaded',
          data,
        })
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setResult({
          state: 'unavailable',
          data: null,
        })
      })

    return () => {
      abortController.abort()
    }
  }, [])

  return result
}
