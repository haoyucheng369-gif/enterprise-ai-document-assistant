import { Activity, FileText, Quote, Tags, Workflow } from 'lucide-react'
import { useState } from 'react'
import type {
  Citation,
  ClassificationSkillResponse,
  DocumentItem,
  DocumentReviewWorkflowResponse,
  ToolResult,
} from '../../types'
import { CitationPanel } from '../insights/CitationPanel'
import { ClassificationResultPanel } from '../insights/ClassificationResultPanel'
import { DocumentPreview } from './DocumentPreview'
import { ToolResultPanel } from '../insights/ToolResultPanel'
import { WorkflowResultPanel } from '../insights/WorkflowResultPanel'
import { WorkspaceHeader } from '../layout/WorkspaceHeader'

type DocumentWorkspaceProps = {
  document: DocumentItem
  citations: Citation[]
  toolResult: ToolResult
  classificationResult: ClassificationSkillResponse | null
  classificationState: 'idle' | 'running' | 'failed'
  workflowResult: DocumentReviewWorkflowResponse | null
  workflowState: 'idle' | 'running' | 'failed'
  onClassifyDocument: () => Promise<void>
  onRunWorkflow: () => Promise<void>
}

type WorkspaceTab = 'preview' | 'classification' | 'workflow' | 'citations' | 'tools'

const workspaceTabs: {
  id: WorkspaceTab
  label: string
  icon: typeof FileText
}[] = [
  { id: 'preview', label: 'Preview', icon: FileText },
  { id: 'classification', label: 'Classification', icon: Tags },
  { id: 'workflow', label: 'Workflow', icon: Workflow },
  { id: 'citations', label: 'Citations', icon: Quote },
  { id: 'tools', label: 'Tools', icon: Activity },
]

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
            <DocumentPreview document={document} />
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

          {activeTab === 'citations' ? (
            <CitationPanel citations={citations} />
          ) : null}

          {activeTab === 'tools' ? (
            <ToolResultPanel document={document} toolResult={toolResult} />
          ) : null}
        </div>
      </div>
    </section>
  )
}
