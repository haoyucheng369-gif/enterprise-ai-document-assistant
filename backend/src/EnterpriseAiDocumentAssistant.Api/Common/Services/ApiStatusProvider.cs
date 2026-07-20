using EnterpriseAiDocumentAssistant.Api.Contracts;
using EnterpriseAiDocumentAssistant.Api.Options;
using Microsoft.Extensions.Options;

namespace EnterpriseAiDocumentAssistant.Api.Services;

public sealed class ApiStatusProvider : IApiStatusProvider
{
    private const string ServiceName = "Enterprise AI Document Assistant API";
    private const string ApiVersion = "v1";
    private const string Version = "0.1.0";

    private readonly IHostEnvironment environment;
    private readonly IOptions<AiGatewayOptions> aiGatewayOptions;
    private readonly ISystemClock clock;

    public ApiStatusProvider(
        IHostEnvironment environment,
        IOptions<AiGatewayOptions> aiGatewayOptions,
        ISystemClock clock)
    {
        this.environment = environment;
        this.aiGatewayOptions = aiGatewayOptions;
        this.clock = clock;
    }

    public StatusResponse GetStatus()
    {
        return new StatusResponse(
            ServiceName,
            environment.EnvironmentName,
            ApiVersion,
            Version,
            aiGatewayOptions.Value.Provider,
            clock.UtcNow);
    }
}
