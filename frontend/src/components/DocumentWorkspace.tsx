import type { Citation, DocumentItem, ToolResult } from '../types'
import { CitationPanel } from './CitationPanel'
import { DocumentPreview } from './DocumentPreview'
import { ToolResultPanel } from './ToolResultPanel'
import { WorkspaceHeader } from './WorkspaceHeader'

type DocumentWorkspaceProps = {
  document: DocumentItem
  citations: Citation[]
  toolResult: ToolResult
}

export function DocumentWorkspace({
  document,
  citations,
  toolResult,
}: DocumentWorkspaceProps) {
  return (
    <section
      aria-label="Document workspace"
      className="grid min-w-0 grid-rows-[auto_minmax(0,1fr)] gap-4 p-5"
    >
      <WorkspaceHeader document={document} />

      <div className="grid min-h-0 gap-4 xl:grid-cols-[minmax(0,1fr)_288px]">
        <DocumentPreview document={document} />
        <aside aria-label="Assistant context" className="grid content-start gap-4">
          <CitationPanel citations={citations} />
          <ToolResultPanel toolResult={toolResult} />
        </aside>
      </div>
    </section>
  )
}
