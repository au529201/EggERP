
using EggERP.Application.ActivityLogs;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/activitylogs")]
[Authorize(Roles = "Admin,Manager,Staff")]
[Authorize(Policy = "ActiveBusiness")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivityLogController(
        IActivityLogService activityLogService,
        UserManager<ApplicationUser> userManager)
    {
        _activityLogService = activityLogService;
        _userManager = userManager;
    }

    [HttpGet("paged")]
    public async Task<IActionResult> SearchPaged(
        [FromQuery] string? search,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
            return Unauthorized();

        if (!currentUser.IsActive ||
            currentUser.BusinessId == Guid.Empty)
            return Forbid();

        if (startDate.HasValue &&
            endDate.HasValue &&
            startDate.Value.Date > endDate.Value.Date)
        {
            return BadRequest(
                "Start date cannot be later than end date.");
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(
                "Page must be positive and page size must be 1–100.");
        }

        var filter = new ActivityLogFilter
        {
            BusinessId = currentUser.BusinessId,
            SearchText = search,
            EntityType = entityType,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _activityLogService.SearchPagedAsync(
            filter, page, pageSize);

        return Ok(new
        {
            items = result.Items.Select(l => new
            {
                l.Id,
                l.EntityId,
                l.UserName,
                l.Action,
                l.EntityType,
                l.Details,
                l.TimestampUtc
            }),
            result.TotalCount,
            result.Page,
            result.PageSize
        });
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

        if (!currentUser.IsActive)
        {
            return Forbid();
        }

        if (currentUser.BusinessId == Guid.Empty)
        {
            return Forbid();
        }

        if (startDate.HasValue &&
            endDate.HasValue &&
            startDate.Value.Date > endDate.Value.Date)
        {
            return BadRequest(
                "The start date cannot be later than the end date.");
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
