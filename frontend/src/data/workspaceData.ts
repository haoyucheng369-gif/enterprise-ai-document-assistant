import type { Citation, DocumentItem, Message, ToolResult } from '../types'

export const documents: DocumentItem[] = [
  {
    id: 'contract-review',
    title: 'Vendor Service Agreement',
    type: 'Contract',
    updatedAt: 'Today',
    status: 'Indexed',
    sections: [
      {
        label: 'Section 4. Renewal Terms',
        title: 'Automatic renewal and notice window',
        body: 'The agreement renews automatically for successive twelve-month periods unless either party provides written notice at least thirty days before the current term ends.',
      },
      {
        label: 'Section 7. Liability',
        title: 'Liability cap and exclusions',
        body: "Each party's aggregate liability is limited to fees paid in the previous twelve months. Confidentiality and data protection obligations remain subject to separate remedies.",
      },
      {
        label: 'Section 9. Service Credits',
        title: 'Availability commitments',
        body: 'Service credits apply when monthly availability falls below the agreed threshold. Credits must be requested within fifteen days after the incident report is available.',
      },
    ],
  },
  {
    id: 'security-policy',
    title: 'Information Security Policy',
    type: 'Policy',
    updatedAt: 'Yesterday',
    status: 'Ready',
    sections: [],
  },
  {
    id: 'operations-report',
    title: 'Q3 Operations Report',
    type: 'Report',
    updatedAt: 'Jul 8',
    status: 'Queued',
    sections: [],
  },
]

export const citations: Citation[] = [
  { id: 'c1', label: 'Section 4 - Renewal notice window' },
  { id: 'c2', label: 'Section 7 - Liability cap' },
  { id: 'c3', label: 'Section 9 - Service credit request period' },
]

export const toolResult: ToolResult = {
  name: 'GetDocumentMetadataTool',
  status: 'Ready',
  description: 'Document indexed with 18 chunks and 3 highlighted clauses.',
}

export const messages: Message[] = [
  {
    id: 'm1',
    role: 'assistant',
    content:
      'Select a document and ask a question. I can summarize clauses, identify risks, and prepare follow-up actions.',
  },
  {
    id: 'm2',
    role: 'user',
    content: 'What should I review first in this agreement?',
  },
  {
    id: 'm3',
    role: 'assistant',
    content:
      'Start with renewal terms, liability limits, and service credits. These sections are highlighted in the current document.',
  },
]
