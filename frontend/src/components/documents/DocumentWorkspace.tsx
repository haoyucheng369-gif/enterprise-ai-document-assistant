import type {
  Citation,
  ClassificationSkillResponse,
  DocumentItem,
  ToolResult,
} from '../../types'
import type { DocumentReviewWorkflowResponse } from '../../types'
import { CitationPanel } from '../insights/CitationPanel'
import { ClassificationResultPanel } from '../insights/ClassificationResultPanel'
import { DocumentPreview } from './DocumentPreview'
import { ToolResultPanel } from '../insights/ToolResultPanel'
import { WorkspaceHeader } from '../layout/WorkspaceHeader'
import { WorkflowResultPanel } from '../insights/WorkflowResultPanel'

type DocumentWorkspaceProps = {
  document: DocumentItem
  citations: Citation[]
  classificationResult: ClassificationSkillResponse | null
  classificationState: 'idle' | 'running' | 'failed'
  toolResult: ToolResult
  workflowResult: DocumentReviewWorkflowResponse | null
  workflowState: 'idle' | 'running' | 'failed'
  onClassifyDocument: () => Promise<void>
  onRunWorkflow: () => Promise<void>
}

export function DocumentWorkspace({
  document,
  citations,
  classificationResult,
  classificationState,
  onClassifyDocument,
  onRunWorkflow,
  toolResult,
  workflowResult,
  workflowState,
}: DocumentWorkspaceProps) {
  return (
    <section
      aria-label="Document workspace"
      className="grid min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)] gap-3 overflow-hidden p-4"
    >
      <WorkspaceHeader
        document={document}
        isClassifying={classificationState === 'running'}
        isWorkflowRunning={workflowState === 'running'}
        onClassifyDocument={onClassifyDocument}
        onRunWorkflow={onRunWorkflow}
      />

      <div className="grid min-h-0 gap-3 overflow-hidden xl:grid-cols-[minmax(0,1fr)_248px] 2xl:grid-cols-[minmax(0,1fr)_264px]">
        <DocumentPreview document={document} />
        <aside
          aria-label="Assistant context"
          className="grid min-h-0 content-start gap-3 overflow-y-auto"
        >
          <ClassificationResultPanel
            result={classificationResult}
            state={classificationState}
          />
          <WorkflowResultPanel result={workflowResult} state={workflowState} />
          <CitationPanel citations={citations} />
          <ToolResultPanel toolResult={toolResult} />
        </aside>
      </div>
    </section>
  )
}
