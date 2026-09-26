using EggERP.Application.ActivityLogs;
using EggERP.Application.Flocks;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/stock")]
[Authorize(Roles = "Admin,Manager")]
public class StockController : ControllerBase
{
    private readonly IFlockService _flockService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public StockController(
        IFlockService flockService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _flockService = flockService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process(ProcessStockTransactionRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null) return Unauthorized();

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) = await _flockService.ProcessTransactionAsync(request);
        if (!succeeded) return BadRequest(error);

        var actionLabel = request.Direction == "In" ? "Added Stock" : "Removed Stock";
        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            actionLabel, "Stock", null,
            $"{request.Lines.Count} line(s) processed ({request.Direction})");

        return Ok();
    }
}