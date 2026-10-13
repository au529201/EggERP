
using EggERP.Application.Businesses;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/provisioning")]
[Authorize(Roles = "PlatformAdmin")]
public class BusinessProvisioningController : ControllerBase
{
    private readonly IBusinessProvisioningService _service;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessProvisioningController(
        IBusinessProvisioningService service,
        UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    [HttpPost("business")]
    public async Task<IActionResult> CreateBusiness(
        [FromBody] CreateBusinessOwnerRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
        {
            return Unauthorized();
        }

        if (!await _userManager.IsInRoleAsync(
            currentUser, "PlatformAdmin"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _service
            .CreateBusinessWithOwnerAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(new
        {
            message =
                "Business and owner account created. " +
                "An invitation email has been requested.",
            businessId = result.BusinessId
        });
    }
}
