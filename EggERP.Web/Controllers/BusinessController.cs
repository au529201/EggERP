
using EggERP.Application.ActivityLogs;
using EggERP.Application.Businesses;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/business")]
[Authorize(Roles = "Admin")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public BusinessController(
        IBusinessService businessService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _businessService = businessService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBusiness(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
            return Unauthorized();

        if (id != currentUser.BusinessId)
            return Forbid();

        var business = await _businessService.GetBusinessAsync(
            currentUser.BusinessId);

        return business is null ? NotFound() : Ok(business);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBusiness(
        Guid id,
        Business business)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
            return Unauthorized();

        if (id != currentUser.BusinessId)
            return Forbid();

        if (business is null || business.Id != currentUser.BusinessId)
            return BadRequest("Invalid business identifier.");

        var updated = await _businessService.UpdateBusinessAsync(business);

        if (!updated)
            return NotFound();

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Updated Business Settings",
            "Business",
            currentUser.BusinessId,
            null);

        return NoContent();
    }
}
