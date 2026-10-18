using Microsoft.AspNetCore.Http;

namespace EggERP.Web.Services;

public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Uri _apiBaseUri;

    public CookieForwardingHandler(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;

        var apiBaseUrl = configuration["ApiBaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl) ||
            !Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                "A valid ApiBaseUrl configuration is required.");
        }

        _apiBaseUri = uri;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var destination = request.RequestUri;

        if (destination is not null &&
            destination.Scheme == _apiBaseUri.Scheme &&
            destination.Host.Equals(
                _apiBaseUri.Host,
                StringComparison.OrdinalIgnoreCase) &&
            destination.Port == _apiBaseUri.Port &&
            destination.AbsolutePath.StartsWith(
                "/api/",
                StringComparison.OrdinalIgnoreCase))
        {
            var cookie = _httpContextAccessor.HttpContext?
                .Request.Headers.Cookie.ToString();

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.Remove("Cookie");
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}