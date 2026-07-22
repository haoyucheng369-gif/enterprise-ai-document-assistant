using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class WorkspaceDataProvider : IWorkspaceDataProvider
{
    public WorkspaceResponse GetWorkspace()
    {
        return new WorkspaceResponse(
            Documents:
            [
                new DocumentItemResponse(
                    "contract-review",
                    "Vendor Service Agreement",
                    "Contract",
                    "Today",
                    "Indexed",
                    [
                        new DocumentSectionResponse(
                            "Section 4. Renewal Terms",
                            "Automatic renewal and notice window",
                            "The agreement renews automatically for successive twelve-month periods unless either party provides written notice at least thirty days before the current term ends."),
                        new DocumentSectionResponse(
                            "Section 7. Liability",
                            "Liability cap and exclusions",
                            "Each party's aggregate liability is limited to fees paid in the previous twelve months. Confidentiality and data protection obligations remain subject to separate remedies."),
                        new DocumentSectionResponse(
                            "Section 9. Service Credits",
                            "Availability commitments",
                            "Service credits apply when monthly availability falls below the agreed threshold. Credits must be requested within fifteen days after the incident report is available.")
                    ]),
                new DocumentItemResponse(
                    "security-policy",
                    "Information Security Policy",
                    "Policy",
                    "Yesterday",
                    "Ready",
                    []),
                new DocumentItemResponse(
                    "operations-report",
                    "Q3 Operations Report",
                    "Report",
                    "Jul 8",
                    "Queued",
                    [])
            ],
            Messages:
            [
                new MessageResponse(
                    "m1",
                    "assistant",
                    "Select a document and ask a question. I can summarize, classify, review risks, and suggest follow-up actions.")
            ],
            Citations:
            [
                new CitationResponse("c1", "Section 4 - Renewal notice window"),
                new CitationResponse("c2", "Section 7 - Liability cap"),
                new CitationResponse("c3", "Section 9 - Service credit request period")
            ],
            ToolResult: new ToolResultResponse(
                "GetDocumentMetadataTool",
                "Ready",
                "Document indexed with 18 chunks and 3 highlighted clauses."));
    }
}
