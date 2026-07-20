namespace EnterpriseAiDocumentAssistant.Api.Services;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
