using EggERP.Application.ActivityLogs;
using EggERP.Application.Flocks;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EggERP.Application.Flocks;
namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/eggs")]
[Authorize]
public class EggController : ControllerBase
{
    private const string Group = "Egg";

    private readonly IFlockService _flockService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public EggController(
        IFlockService flockService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _flockService = flockService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet("board")]
    public async Task<IActionResult> GetBoard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var board = await _flockService.GetBoardAsync(currentUser.BusinessId, Group);
        return Ok(board);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddStock(AddStockRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.AddStockAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Added Egg Stock", "Egg", request.ProductId,
            $"{request.Reason}: +{request.Quantity}");

        return Ok();
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveStock(RemoveStockRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.RemoveStockAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Removed Egg Stock", "Egg", request.ProductId,
            $"{request.Reason}: -{request.Quantity}");

        return Ok();
    }
}