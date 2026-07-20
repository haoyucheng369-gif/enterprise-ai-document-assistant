namespace EnterpriseAiDocumentAssistant.Api.Harness;

public sealed record HarnessReport(
    bool Succeeded,
    int Passed,
    int Failed,
    IReadOnlyList<HarnessCheckResult> Checks);

public sealed record HarnessCheckResult(
    string Name,
    bool Passed,
    string Detail);
