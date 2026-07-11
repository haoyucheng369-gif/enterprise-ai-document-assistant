import { CircleAlert, CircleCheck, LoaderCircle } from 'lucide-react'
import type { ApiConnectionState, ApiStatusResponse } from '../../types'

type ApiStatusBadgeProps = {
  state: ApiConnectionState
  status: ApiStatusResponse | null
}

const stateStyles: Record<ApiConnectionState, string> = {
  loading: 'border-amber-200 bg-amber-50 text-amber-700',
  connected: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  unavailable: 'border-rose-200 bg-rose-50 text-rose-700',
}

export function ApiStatusBadge({ state, status }: ApiStatusBadgeProps) {
  const label =
    state === 'connected'
      ? `${status?.environment ?? 'Unknown'} / ${status?.apiVersion ?? 'v?' }`
      : state === 'loading'
        ? 'Checking API'
        : 'API unavailable'

  return (
    <div
      className={`inline-flex items-center gap-2 rounded-md border px-2 py-1 text-xs font-medium ${stateStyles[state]}`}
      title={status ? `${status.service} - ${status.aiProvider}` : undefined}
    >
      {state === 'loading' && <LoaderCircle className="animate-spin" size={14} />}
      {state === 'connected' && <CircleCheck size={14} />}
      {state === 'unavailable' && <CircleAlert size={14} />}
      <span>{label}</span>
    </div>
  )
}
