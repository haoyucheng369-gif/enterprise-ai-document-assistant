namespace EnterpriseAiDocumentAssistant.Api.RateLimiting;

public static class AiRateLimitPolicy
{
    public const string Name = "ai-operations";
    public const int PermitLimit = 10;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
}
