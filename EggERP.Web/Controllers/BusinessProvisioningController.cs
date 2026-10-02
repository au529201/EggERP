using EggERP.Application.Businesses;
using EggERP.Infrastructure.Businesses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/provisioning")]
[AllowAnonymous]
public class BusinessProvisioningController : ControllerBase
{
    private readonly IBusinessProvisioningService
        _businessProvisioningService;

    private readonly IConfiguration _configuration;

    public BusinessProvisioningController(
        IBusinessProvisioningService businessProvisioningService,
        IConfiguration configuration)
    {
        _businessProvisioningService =
            businessProvisioningService;

        _configuration = configuration;
    }

    [HttpPost("business")]
    public async Task<IActionResult> CreateBusiness(
        CreateBusinessOwnerRequest request)
    {
        var configuredKey =
            _configuration["Provisioning:Key"];

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Business provisioning is not configured.");
        }

        if (!Request.Headers.TryGetValue(
                "X-Provisioning-Key",
                out var suppliedKey))
        {
            return Unauthorized();
        }

        if (!string.Equals(
                suppliedKey.ToString(),
                configuredKey,
                StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _businessProvisioningService
                .CreateBusinessWithOwnerAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                error = result.Error,
                businessId = result.BusinessId
            });
        }

        return Ok(new
        {
            message =
                "Business created successfully. " +
                "An activation email has been sent to the owner.",

            businessId = result.BusinessId
        });
    }
}