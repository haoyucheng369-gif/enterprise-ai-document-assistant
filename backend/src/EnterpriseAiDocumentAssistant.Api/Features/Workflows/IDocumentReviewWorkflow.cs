namespace EnterpriseAiDocumentAssistant.Api.Workflows;

// Workflow services coordinate multiple capabilities behind one application-level use case.
public interface IDocumentReviewWorkflow
{
    DocumentReviewWorkflowResponse? Run(DocumentReviewWorkflowRequest request);

    Task<DocumentReviewWorkflowResponse?> RunAsync(
        DocumentReviewWorkflowRequest request,
        CancellationToken cancellationToken);
}
