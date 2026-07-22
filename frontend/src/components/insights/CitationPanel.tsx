import { Quote } from 'lucide-react'
import { useState } from 'react'
import type { Citation } from '../../types'

type CitationPanelProps = {
  citations: Citation[]
}

export function CitationPanel({ citations }: CitationPanelProps) {
  const [isExpanded, setIsExpanded] = useState(false)
  const visibleCitations = isExpanded ? citations : citations.slice(0, 5)
  const hiddenCitationCount = Math.max(citations.length - visibleCitations.length, 0)

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
        {visibleCitations.map((citation) => (
          <li
            className="border-l-2 border-sky-300 pl-3 text-sm leading-6 text-slate-700"
            key={citation.id}
          >
            {citation.label}
          </li>
        ))}
      </ul>
      {hiddenCitationCount > 0 ? (
        <button
          className="mt-3 cursor-pointer text-xs font-medium text-sky-700 hover:text-sky-800"
          onClick={() => setIsExpanded(true)}
          type="button"
        >
          Show {hiddenCitationCount} more
        </button>
      ) : isExpanded && citations.length > 5 ? (
        <button
          className="mt-3 cursor-pointer text-xs font-medium text-sky-700 hover:text-sky-800"
          onClick={() => setIsExpanded(false)}
          type="button"
        >
          Show less
        </button>
      ) : null}
    </section>
  )
}
