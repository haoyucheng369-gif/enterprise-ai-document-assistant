namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
