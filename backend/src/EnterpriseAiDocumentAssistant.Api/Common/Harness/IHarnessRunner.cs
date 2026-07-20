namespace EnterpriseAiDocumentAssistant.Api.Harness;

public interface IHarnessRunner
{
    Task<HarnessReport> RunAsync(CancellationToken cancellationToken);
}
