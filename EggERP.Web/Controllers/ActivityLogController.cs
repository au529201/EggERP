using EggERP.Application.ActivityLogs;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/activitylogs")]
[Authorize(Roles = "Admin,Manager")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivityLogController(IActivityLogService activityLogService, UserManager<ApplicationUser> userManager)
    {
        _activityLogService = activityLogService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var filter = new ActivityLogFilter
        {
            BusinessId = currentUser.BusinessId,
            SearchText = search,
            EntityType = entityType,
            StartDate = startDate,
            EndDate = endDate
        };

        var logs = await _activityLogService.SearchAsync(filter);
        return Ok(logs);
    }
}