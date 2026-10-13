
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/current-business")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class CurrentBusinessController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentBusinessController(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentBusiness()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive ||
            user.BusinessId == Guid.Empty)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            businessId = user.BusinessId
        });
    }
}
