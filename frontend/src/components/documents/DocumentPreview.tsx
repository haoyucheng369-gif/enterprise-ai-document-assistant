import type { DocumentItem } from '../../types'
import { useState } from 'react'

type DocumentPreviewProps = {
  document: DocumentItem
  maxSections?: number
}

export function DocumentPreview({
  document,
  maxSections = document.sections.length,
}: DocumentPreviewProps) {
  const [isExpanded, setIsExpanded] = useState(false)
  const visibleSections = isExpanded
    ? document.sections
    : document.sections.slice(0, maxSections)
  const hiddenSectionCount = Math.max(document.sections.length - visibleSections.length, 0)

  return (
    <article className="min-w-0 rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-start justify-between gap-3 border-b border-slate-100 pb-3">
        <div className="min-w-0">
          <p className="text-xs font-semibold uppercase tracking-[0.06em] text-blue-600">
            Document overview
          </p>
          <h3 className="mt-1 truncate text-base font-semibold text-slate-950">
            {document.title}
          </h3>
        </div>
        <span className="rounded-sm bg-emerald-50 px-2 py-0.5 text-[11px] font-medium text-emerald-700">
          {document.status}
        </span>
      </div>

      <dl className="mt-3 grid grid-cols-2 gap-2 text-xs text-slate-500">
        <div>
          <dt className="font-medium text-slate-700">Type</dt>
          <dd className="mt-0.5">{document.type}</dd>
        </div>
        <div>
          <dt className="font-medium text-slate-700">Updated</dt>
          <dd className="mt-0.5">{document.updatedAt}</dd>
        </div>
      </dl>

      <div className="mt-4 grid gap-4">
        {visibleSections.map((section) => (
          <section key={section.label}>
            <p className="text-xs font-semibold uppercase tracking-[0.06em] text-blue-600">
              {section.label}
            </p>
            <h3 className="mt-1.5 text-base font-semibold text-slate-950">
              {section.title}
            </h3>
            <p className="mt-1.5 text-sm leading-6 text-slate-700">
              {section.body}
            </p>
          </section>
        ))}
      </div>

      {hiddenSectionCount > 0 ? (
        <button
          className="mt-4 w-full cursor-pointer rounded-md border border-slate-100 bg-slate-50 px-3 py-2 text-left text-xs font-medium text-slate-600 hover:border-slate-200 hover:bg-slate-100"
          onClick={() => setIsExpanded(true)}
          type="button"
        >
          Show {hiddenSectionCount} additional chunks
        </button>
      ) : isExpanded && document.sections.length > maxSections ? (
        <button
          className="mt-4 w-full cursor-pointer rounded-md border border-slate-100 bg-slate-50 px-3 py-2 text-left text-xs font-medium text-slate-600 hover:border-slate-200 hover:bg-slate-100"
          onClick={() => setIsExpanded(false)}
          type="button"
        >
          Show less
        </button>
      ) : null}
    </article>
  )
}
