import { FilePenLine } from 'lucide-react'
import type { ResumeReviewSkillResponse } from '../../types'

type ResumeReviewPanelProps = {
  onGenerateReview: () => Promise<void>
  result: ResumeReviewSkillResponse | null
  state: 'idle' | 'running' | 'failed'
}

export function ResumeReviewPanel({
  onGenerateReview,
  result,
  state,
}: ResumeReviewPanelProps) {
  return (
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div>
          <h3 className="inline-flex items-center gap-2 text-base font-semibold text-slate-900">
            <FilePenLine className="text-blue-600" size={17} />
            Review
          </h3>
          <p className="mt-1 text-sm text-slate-600">
            Generate a Markdown review brief from the selected resume.
          </p>
        </div>
        <button
          className="inline-flex cursor-pointer items-center rounded-md border border-blue-200 bg-blue-50 px-3 py-1.5 text-xs font-semibold text-blue-700 hover:bg-blue-100 disabled:cursor-not-allowed disabled:border-slate-200 disabled:bg-slate-100 disabled:text-slate-400"
          disabled={state === 'running'}
          onClick={() => void onGenerateReview()}
          type="button"
        >
          {state === 'running' ? 'Reviewing' : 'Generate Review'}
        </button>
      </div>

      {state === 'failed' ? (
        <div className="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          Resume review request failed. Check the selected provider and try again.
        </div>
      ) : null}

      {result === null && state !== 'failed' ? (
        <div className="rounded-md border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
          No resume review generated yet.
        </div>
      ) : null}

      {result !== null ? (
        <div className="grid gap-3">
          <div className="rounded-md border border-blue-100 bg-blue-50 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-blue-700">
              {result.format} / {result.provider}
            </p>
            <h4 className="mt-1 text-sm font-semibold text-slate-950">
              {result.title}
            </h4>
          </div>
          <pre className="max-h-[64vh] overflow-auto whitespace-pre-wrap rounded-md border border-slate-200 bg-white p-4 text-sm leading-6 text-slate-800">
            {result.content}
          </pre>
          {result.nextActions.length > 0 ? (
            <div className="flex flex-wrap gap-2">
              {result.nextActions.map((action) => (
                <span
                  className="rounded-md border border-indigo-100 bg-indigo-50 px-2.5 py-1 text-xs font-medium text-indigo-700"
                  key={action}
                >
                  {action}
                </span>
              ))}
            </div>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
