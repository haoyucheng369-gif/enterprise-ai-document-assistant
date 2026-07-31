namespace EnterpriseAiDocumentAssistant.Api.Security;

public sealed class HttpHeaderCurrentUserAccessor : ICurrentUserAccessor
{
    public const string HeaderName = "X-User-Id";
    public const string DefaultUserId = "local-user";

    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpHeaderCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            // The header is a local identity adapter. Production deployments replace it with authenticated claims.
            var headerValue = httpContextAccessor.HttpContext?
                .Request.Headers[HeaderName]
                .FirstOrDefault()?
                .Trim();

            return string.IsNullOrWhiteSpace(headerValue)
                ? DefaultUserId
                : headerValue.ToLowerInvariant();
        }
    }
}
