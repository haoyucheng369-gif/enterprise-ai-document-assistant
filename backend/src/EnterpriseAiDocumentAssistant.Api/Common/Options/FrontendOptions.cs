namespace EnterpriseAiDocumentAssistant.Api.Options;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";
    public const string CorsPolicyName = "Frontend";

    public string[] AllowedOrigins { get; init; } =
    [
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    ];
}
