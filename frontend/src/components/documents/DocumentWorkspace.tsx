import type { Citation, DocumentItem, ToolResult } from '../../types'
import { CitationPanel } from '../insights/CitationPanel'
import { DocumentPreview } from './DocumentPreview'
import { ToolResultPanel } from '../insights/ToolResultPanel'
import { WorkspaceHeader } from '../layout/WorkspaceHeader'

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
      className="grid min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)] gap-4 overflow-hidden p-5"
    >
      <WorkspaceHeader document={document} />

      <div className="grid min-h-0 gap-4 overflow-hidden xl:grid-cols-[minmax(0,1fr)_288px]">
        <DocumentPreview document={document} />
        <aside
          aria-label="Assistant context"
          className="grid min-h-0 content-start gap-4 overflow-y-auto"
        >
          <CitationPanel citations={citations} />
          <ToolResultPanel toolResult={toolResult} />
        </aside>
      </div>
    </section>
  )
}
