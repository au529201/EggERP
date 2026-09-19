using EggERP.Application.Flocks;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/flocks")]
[Authorize]
public class FlockController : ControllerBase
{
    private readonly IFlockService _flockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FlockController(IFlockService flockService, UserManager<ApplicationUser> userManager)
    {
        _flockService = flockService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlocks()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var flocks = await _flockService.GetFlocksForBusinessAsync(currentUser.BusinessId);
        return Ok(flocks);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFlock(CreateFlockRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.CreateFlockAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost("movements")]
    public async Task<IActionResult> RecordMovement(RecordFlockMovementRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.RecordMovementAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }

        return Ok();
    }
}