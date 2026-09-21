using EggERP.Application.ActivityLogs;
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
    private readonly IActivityLogService _activityLogService;

    public FlockController(
        IFlockService flockService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _flockService = flockService;
        _userManager = userManager;
        _activityLogService = activityLogService;
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

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Created Flock", "Flock", null,
            $"{request.Name} ({request.Species}) — {request.InitialQuantity} birds ({request.InitialSource})");

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

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Recorded Flock Movement", "FlockMovement", request.FlockId,
            $"{request.Direction}/{request.Reason}: {request.Quantity} bird(s)");

        return Ok();
    }

    [HttpPost("egg-movements")]
    public async Task<IActionResult> RecordEggMovement(RecordEggMovementRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.RecordEggMovementAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Recorded Egg Movement", "EggMovement", request.FlockId,
            $"{request.Direction}/{request.Reason}: {request.Quantity} egg(s)");

        return Ok();
    }
}