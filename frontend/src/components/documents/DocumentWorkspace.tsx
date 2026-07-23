import { FilePenLine, FileText, Tags, Workflow } from 'lucide-react'
import { useState } from 'react'
import type {
  Citation,
  ClassificationSkillResponse,
  DocumentItem,
  DocumentReviewWorkflowResponse,
  ResumeReviewSkillResponse,
  ToolResult,
} from '../../types'
import { CitationPanel } from '../insights/CitationPanel'
import { ClassificationResultPanel } from '../insights/ClassificationResultPanel'
import { DocumentPreview } from './DocumentPreview'
import { ToolResultPanel } from '../insights/ToolResultPanel'
import { WorkflowResultPanel } from '../insights/WorkflowResultPanel'
import { ResumeReviewPanel } from '../insights/ResumeReviewPanel'
import { WorkspaceHeader } from '../layout/WorkspaceHeader'

type DocumentWorkspaceProps = {
  document: DocumentItem
  citations: Citation[]
  toolResult: ToolResult
  classificationResult: ClassificationSkillResponse | null
  classificationState: 'idle' | 'running' | 'failed'
  reviewResult: ResumeReviewSkillResponse | null
  reviewState: 'idle' | 'running' | 'failed'
  workflowResult: DocumentReviewWorkflowResponse | null
  workflowState: 'idle' | 'running' | 'failed'
  onClassifyDocument: () => Promise<void>
  onGenerateResumeReview: () => Promise<void>
  onRunWorkflow: () => Promise<void>
}

type WorkspaceTab = 'preview' | 'classification' | 'workflow' | 'review'

const workspaceTabs: {
  id: WorkspaceTab
  label: string
  icon: typeof FileText
}[] = [
  { id: 'preview', label: 'Preview', icon: FileText },
  { id: 'classification', label: 'Classification', icon: Tags },
  { id: 'workflow', label: 'Workflow', icon: Workflow },
  { id: 'review', label: 'Review', icon: FilePenLine },
]

export function DocumentWorkspace({
  document,
  reviewResult,
  reviewState,
  citations,
  classificationResult,
  classificationState,
  onClassifyDocument,
  onGenerateResumeReview,
  onRunWorkflow,
  toolResult,
  workflowResult,
  workflowState,
}: DocumentWorkspaceProps) {
  const [activeTab, setActiveTab] = useState<WorkspaceTab>('preview')

  return (
    <section
      aria-label="Document context"
      className="grid min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)] gap-3 overflow-hidden border-r border-slate-200 bg-slate-50 p-4"
    >
      <WorkspaceHeader
        document={document}
      />

      <div className="grid min-h-0 grid-rows-[auto_minmax(0,1fr)] overflow-hidden rounded-md border border-slate-200 bg-white">
        <div className="flex gap-1 overflow-x-auto border-b border-slate-200 bg-slate-50 p-2">
          {workspaceTabs.map((tab) => {
            const Icon = tab.icon
            const isActive = activeTab === tab.id

            return (
              <button
                className={`inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium transition ${
                  isActive
                    ? 'bg-white text-blue-700 shadow-sm ring-1 ring-blue-200'
                    : 'text-slate-500 hover:bg-white hover:text-slate-800'
                }`}
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                type="button"
              >
                <Icon size={14} />
                {tab.label}
              </button>
            )
          })}
        </div>

        <div className="min-h-0 overflow-y-auto p-4">
          {activeTab === 'preview' ? (
            <div className="grid gap-4">
              <DocumentPreview document={document} />
              <div className="grid gap-4 2xl:grid-cols-[minmax(0,1.2fr)_minmax(260px,0.8fr)]">
                <CitationPanel citations={citations} />
                <ToolResultPanel document={document} toolResult={toolResult} />
              </div>
            </div>
          ) : null}

          {activeTab === 'classification' ? (
            <ClassificationResultPanel
              onClassifyDocument={onClassifyDocument}
              result={classificationResult}
              state={classificationState}
            />
          ) : null}

          {activeTab === 'workflow' ? (
            <WorkflowResultPanel
              onRunWorkflow={onRunWorkflow}
              result={workflowResult}
              state={workflowState}
            />
          ) : null}

          {activeTab === 'review' ? (
            <ResumeReviewPanel
              onGenerateReview={onGenerateResumeReview}
              result={reviewResult}
              state={reviewState}
            />
          ) : null}

        </div>
      </div>
    </section>
  )
}
