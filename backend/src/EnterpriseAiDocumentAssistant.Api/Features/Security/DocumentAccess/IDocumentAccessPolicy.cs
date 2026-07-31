namespace EnterpriseAiDocumentAssistant.Api.Security;

public interface IDocumentAccessPolicy
{
    DocumentAccessLevel Evaluate(string documentId);
}

public enum DocumentAccessLevel
{
    NotFound,
    Denied,
    Reader,
    Owner
}
