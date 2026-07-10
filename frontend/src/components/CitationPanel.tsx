import { Quote } from 'lucide-react'
import type { Citation } from '../types'

type CitationPanelProps = {
  citations: Citation[]
}

export function CitationPanel({ citations }: CitationPanelProps) {
  return (
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-center justify-between">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
          <Quote className="text-sky-600" size={16} />
          Citations
        </h3>
        <span className="rounded-sm bg-sky-50 px-2 py-0.5 text-[11px] font-medium text-sky-700">
          {citations.length}
        </span>
      </div>
      <ul className="mt-4 grid gap-3">
        {citations.map((citation) => (
          <li
            className="border-l-2 border-sky-300 pl-3 text-sm leading-6 text-slate-700"
            key={citation.id}
          >
            {citation.label}
          </li>
        ))}
      </ul>
    </section>
  )
}
