import { Tags } from 'lucide-react'
import type { ClassificationSkillResponse } from '../../types'

type ClassificationResultPanelProps = {
  result: ClassificationSkillResponse | null
  state: 'idle' | 'running' | 'failed'
  onClassifyDocument: () => Promise<void>
}

export function ClassificationResultPanel({
  onClassifyDocument,
  result,
  state,
}: ClassificationResultPanelProps) {
  return (
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-center justify-between">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
          <Tags className="text-blue-600" size={16} />
          Classification
        </h3>
        <div className="flex items-center gap-2">
          <span
            className={`rounded-sm px-2 py-0.5 text-[11px] font-medium ${
              state === 'failed'
                ? 'bg-rose-50 text-rose-700'
                : state === 'running'
                  ? 'bg-amber-50 text-amber-700'
                  : result
                    ? 'bg-blue-50 text-blue-700'
                    : 'bg-slate-100 text-slate-500'
            }`}
          >
            {state === 'running'
              ? 'Running'
              : state === 'failed'
                ? 'Failed'
                : result?.provider ?? 'Ready'}
          </span>
          <button
            className="cursor-pointer rounded-md border border-blue-200 bg-blue-50 px-2 py-1 text-xs font-medium text-blue-800 hover:bg-blue-100 disabled:cursor-wait disabled:opacity-70"
            disabled={state === 'running'}
            onClick={() => void onClassifyDocument()}
            type="button"
          >
            Run
          </button>
        </div>
      </div>

      {state === 'failed' ? (
        <p className="mt-4 rounded-md border border-rose-100 bg-rose-50 p-3 text-sm leading-6 text-rose-700">
          Classification request failed. Check provider settings and try again.
        </p>
      ) : null}

      {result ? (
        <div className="mt-4 grid gap-3">
          <div className="rounded-md border border-blue-100 bg-blue-50/70 p-3">
            <p className="text-xs font-semibold uppercase tracking-[0.06em] text-blue-700">
              {result.category}
            </p>
            <p className="mt-1 text-sm text-slate-700">
              Priority: <span className="font-medium">{result.priority}</span>
              <span className="text-slate-400"> / </span>
              Confidence: {Math.round(result.confidence * 100)}%
            </p>
          </div>
          <p className="text-sm leading-6 text-slate-700">{result.reason}</p>
          {result.signals.length > 0 ? (
            <ul className="grid gap-1">
              {result.signals.slice(0, 3).map((signal) => (
                <li className="text-xs leading-5 text-slate-500" key={signal}>
                  {signal}
                </li>
              ))}
            </ul>
          ) : null}
        </div>
      ) : state === 'idle' ? (
        <p className="mt-4 text-sm leading-6 text-slate-500">
          Classify the selected document into a practical business category.
        </p>
      ) : null}
    </section>
  )
}
