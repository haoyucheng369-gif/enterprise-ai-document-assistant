import { AssistantPanel } from './components/assistant/AssistantPanel'
import { DocumentNav } from './components/documents/DocumentNav'
import { DocumentWorkspace } from './components/documents/DocumentWorkspace'
import { useApiStatus } from './hooks/useApiStatus'
import { useWorkspaceData } from './hooks/useWorkspaceData'

function App() {
  const apiStatus = useApiStatus()
  const workspace = useWorkspaceData()

  if (workspace.state === 'loading') {
    return (
      <main className="grid min-h-screen place-items-center bg-slate-100 text-slate-700">
        <div className="rounded-md border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          Loading workspace data...
        </div>
      </main>
    )
  }

  if (workspace.state === 'unavailable' || workspace.data === null) {
    return (
      <main className="grid min-h-screen place-items-center bg-slate-100 text-slate-700">
        <div className="rounded-md border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          Workspace data is unavailable. Start the API and refresh the page.
        </div>
      </main>
    )
  }

  const { citations, documents, messages, toolResult } = workspace.data
  const selectedDocument = documents[0]

  return (
    <main className="grid min-h-screen grid-cols-1 bg-slate-100 text-slate-900 lg:grid-cols-[248px_minmax(0,1fr)] xl:grid-cols-[264px_minmax(0,1fr)_340px]">
      <DocumentNav documents={documents} selectedDocumentId={selectedDocument.id} />
      <DocumentWorkspace
        citations={citations}
        document={selectedDocument}
        toolResult={toolResult}
      />
      <AssistantPanel
        apiState={apiStatus.state}
        apiStatus={apiStatus.status}
        messages={messages}
      />
    </main>
  )
}

export default App
