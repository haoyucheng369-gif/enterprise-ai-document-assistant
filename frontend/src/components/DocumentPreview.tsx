import type { DocumentItem } from '../types'

type DocumentPreviewProps = {
  document: DocumentItem
}

export function DocumentPreview({ document }: DocumentPreviewProps) {
  return (
    <article className="min-w-0 overflow-auto">
      <div className="mx-auto min-h-auto max-w-[760px] rounded-md border border-slate-200 bg-white p-7 xl:min-h-[calc(100vh-126px)] xl:p-10">
        {document.sections.map((section) => (
          <section className="mt-7 first:mt-0" key={section.label}>
            <p className="text-xs font-semibold uppercase tracking-[0.06em] text-blue-600">
              {section.label}
            </p>
            <h3 className="mt-2 text-lg font-semibold text-slate-950">
              {section.title}
            </h3>
            <p className="mt-2 leading-7 text-slate-700">{section.body}</p>
          </section>
        ))}
      </div>
    </article>
  )
}
