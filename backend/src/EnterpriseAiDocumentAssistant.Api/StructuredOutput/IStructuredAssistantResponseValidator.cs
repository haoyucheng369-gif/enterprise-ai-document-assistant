using EnterpriseAiDocumentAssistant.Api.Contracts;

namespace EnterpriseAiDocumentAssistant.Api.StructuredOutput;

public interface IStructuredAssistantResponseValidator
{
    StructuredOutputValidationResult Validate(StructuredAssistantMessage message);
}
