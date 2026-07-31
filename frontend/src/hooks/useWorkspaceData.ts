import { useEffect, useState } from 'react'
import { getWorkspaceData } from '../api/workspaceApi'
import type { DataConnectionState, WorkspaceResponse } from '../types'
import type { UserIdentity } from '../types'

type WorkspaceDataResult = {
  state: DataConnectionState
  data: WorkspaceResponse | null
}

type UserWorkspaceDataResult = WorkspaceDataResult & {
  userId: UserIdentity
}

export function useWorkspaceData(userId: UserIdentity): WorkspaceDataResult {
  const [result, setResult] = useState<UserWorkspaceDataResult>({
    userId,
    state: 'loading',
    data: null,
  })

  useEffect(() => {
    const abortController = new AbortController()
    setResult({ userId, state: 'loading', data: null })

    getWorkspaceData(userId, abortController.signal)
      .then((data) => {
        setResult({
          userId,
          state: 'loaded',
          data,
        })
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setResult({
          userId,
          state: 'unavailable',
          data: null,
        })
      })

    return () => {
      abortController.abort()
    }
  }, [userId])

  // Never expose the previous user's workspace during the render before the reload effect runs.
  return result.userId === userId
    ? result
    : { state: 'loading', data: null }
}
