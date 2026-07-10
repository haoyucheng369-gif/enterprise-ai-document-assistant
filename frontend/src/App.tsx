import { AssistantPanel } from './components/AssistantPanel'
import { DocumentNav } from './components/DocumentNav'
import { DocumentWorkspace } from './components/DocumentWorkspace'
import {
  citations,
  documents,
  messages,
  toolResult,
} from './data/workspaceData'

function App() {
  const selectedDocument = documents[0]

  return (
    <main className="grid min-h-screen grid-cols-1 bg-slate-100 text-slate-900 lg:grid-cols-[248px_minmax(0,1fr)] xl:grid-cols-[264px_minmax(0,1fr)_340px]">
      <DocumentNav documents={documents} selectedDocumentId={selectedDocument.id} />
      <DocumentWorkspace
        citations={citations}
        document={selectedDocument}
        toolResult={toolResult}
      />
      <AssistantPanel messages={messages} />
    </main>
  )
}

export default App
