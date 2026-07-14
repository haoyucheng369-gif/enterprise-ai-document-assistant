import type { DocumentItem } from '../../types'

type DocumentPreviewProps = {
  document: DocumentItem
}

export function DocumentPreview({ document }: DocumentPreviewProps) {
  return (
    <article className="min-h-0 min-w-0 overflow-y-auto">
      <div className="mx-auto max-w-[680px] rounded-md border border-slate-200 bg-white p-5 xl:min-h-full xl:p-6">
        {document.sections.map((section) => (
          <section className="mt-5 first:mt-0" key={section.label}>
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
    </article>
  )
}
