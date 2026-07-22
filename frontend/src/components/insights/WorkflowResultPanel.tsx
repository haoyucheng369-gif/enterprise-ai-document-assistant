import { CheckCircle2, Mail, ShieldAlert, Workflow } from 'lucide-react'
import type { DocumentReviewWorkflowResponse } from '../../types'

type WorkflowResultPanelProps = {
  result: DocumentReviewWorkflowResponse | null
  state: 'idle' | 'running' | 'failed'
  onRunWorkflow: () => Promise<void>
}

export function WorkflowResultPanel({
  onRunWorkflow,
  result,
  state,
}: WorkflowResultPanelProps) {
  return (
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-center justify-between">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
          <Workflow className="text-indigo-600" size={16} />
          Workflow
        </h3>
        <div className="flex items-center gap-2">
          <span
            className={`rounded-sm px-2 py-0.5 text-[11px] font-medium ${
              state === 'failed'
                ? 'bg-rose-50 text-rose-700'
                : state === 'running'
                  ? 'bg-amber-50 text-amber-700'
                  : result
                    ? 'bg-emerald-50 text-emerald-700'
                    : 'bg-slate-100 text-slate-500'
            }`}
          >
            {state === 'running'
              ? 'Running'
              : state === 'failed'
                ? 'Failed'
                : result?.status ?? 'Ready'}
          </span>
          <button
            className="cursor-pointer rounded-md border border-indigo-200 bg-indigo-50 px-2 py-1 text-xs font-medium text-indigo-800 hover:bg-indigo-100 disabled:cursor-wait disabled:opacity-70"
            disabled={state === 'running'}
            onClick={() => void onRunWorkflow()}
            type="button"
          >
            Run
          </button>
        </div>
      </div>

      {state === 'failed' ? (
        <p className="mt-4 rounded-md border border-rose-100 bg-rose-50 p-3 text-sm leading-6 text-rose-700">
          Workflow request failed. Check that the API is running and try again.
        </p>
      ) : null}

      {result ? (
        <div className="mt-4 grid gap-4">
          <ol className="grid gap-2">
            {result.steps.map((step) => (
              <li
                className="grid grid-cols-[18px_minmax(0,1fr)] gap-2 text-sm"
                key={step.name}
              >
                <CheckCircle2 className="mt-0.5 text-emerald-600" size={16} />
                <span>
                  <span className="block font-medium text-slate-900">
                    {step.name}
                  </span>
                  <span className="text-xs leading-5 text-slate-500">
                    {step.detail}
                  </span>
                </span>
              </li>
            ))}
          </ol>

          <div className="rounded-md border border-indigo-100 bg-indigo-50/60 p-3">
            <p className="text-xs font-semibold uppercase tracking-[0.06em] text-indigo-700">
              Summary
            </p>
            <p className="mt-1 text-sm leading-6 text-slate-700">
              {result.summary.summary}
            </p>
          </div>

          <div className="rounded-md border border-amber-100 bg-amber-50/60 p-3">
            <p className="inline-flex items-center gap-1.5 text-xs font-semibold uppercase tracking-[0.06em] text-amber-700">
              <ShieldAlert size={14} />
              Risks
            </p>
            <ul className="mt-2 grid gap-2">
              {result.riskAnalysis.risks.slice(0, 3).map((risk) => (
                <li className="text-sm leading-6 text-slate-700" key={risk.title}>
                  <span className="font-medium text-slate-900">{risk.title}</span>
                  <span className="text-slate-500"> / {risk.severity}</span>
                </li>
              ))}
            </ul>
          </div>

          <div className="rounded-md border border-sky-100 bg-sky-50/60 p-3">
            <p className="inline-flex items-center gap-1.5 text-xs font-semibold uppercase tracking-[0.06em] text-sky-700">
              <Mail size={14} />
              Email draft
            </p>
            <p className="mt-1 text-sm font-medium text-slate-900">
              {result.emailDraft.subject}
            </p>
            <p className="mt-1 line-clamp-4 whitespace-pre-line text-sm leading-6 text-slate-700">
              {result.emailDraft.body}
            </p>
          </div>
        </div>
      ) : result === null && state === 'idle' ? (
        <p className="mt-4 text-sm leading-6 text-slate-500">
          Run the document review workflow to generate a summary, risk analysis,
          and follow-up email draft.
        </p>
      ) : null}
    </section>
  )
}
