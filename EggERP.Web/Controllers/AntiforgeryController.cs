
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/antiforgery")]
[Authorize]
public sealed class AntiforgeryController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(
        IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("token")]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public IActionResult GetToken()
    {
        var tokens =
            _antiforgery.GetAndStoreTokens(HttpContext);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            RequestToken = tokens.RequestToken,
            HeaderName = "X-CSRF-TOKEN"
        });
    }
}
