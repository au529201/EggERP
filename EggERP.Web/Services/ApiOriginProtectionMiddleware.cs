
namespace EggERP.Web.Services;

public sealed class ApiOriginProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiOriginProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Only protect state-changing API operations.
        if (!request.Path.StartsWithSegments("/api") ||
            HttpMethods.IsGet(request.Method) ||
            HttpMethods.IsHead(request.Method) ||
            HttpMethods.IsOptions(request.Method) ||
            HttpMethods.IsTrace(request.Method))
        {
            await _next(context);
            return;
        }

        // Modern browsers identify cross-site requests.
        // Blazor Server's internal HttpClient does not normally
        // send this browser-specific header.
        var fetchSite = request.Headers["Sec-Fetch-Site"].ToString();

        if (string.Equals(
            fetchSite,
            "cross-site",
            StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsync(
                "Cross-site API requests are not permitted.");

            return;
        }

        // If an Origin header is present, require the same
        // scheme, host, and port as the requested application.
        var origin = request.Headers.Origin.ToString();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            if (!Uri.TryCreate(
                    origin,
                    UriKind.Absolute,
                    out var originUri) ||
                (originUri.Scheme != Uri.UriSchemeHttp &&
                 originUri.Scheme != Uri.UriSchemeHttps) ||
                !string.Equals(
                    originUri.Scheme,
                    request.Scheme,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    originUri.Host,
                    request.Host.Host,
                    StringComparison.OrdinalIgnoreCase) ||
                originUri.Port !=
                    (request.Host.Port ??
                     (request.IsHttps ? 443 : 80)))
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                await context.Response.WriteAsync(
                    "Cross-origin API requests are not permitted.");

                return;
            }
        }

        await _next(context);
    }
}
