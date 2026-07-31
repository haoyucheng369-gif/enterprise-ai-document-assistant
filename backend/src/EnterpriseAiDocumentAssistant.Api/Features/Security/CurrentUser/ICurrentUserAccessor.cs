namespace EnterpriseAiDocumentAssistant.Api.Security;

public interface ICurrentUserAccessor
{
    string UserId { get; }
}
