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
        var business = await _businessService.GetBusinessAsync(id);
        if (business is null)
        {
            return NotFound();
        }
        return Ok(business);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBusiness(Guid id, Business business)
    {
        if (id != business.Id)
        {
            return BadRequest("Route id does not match business id in the request body.");
        }
        var updated = await _businessService.UpdateBusinessAsync(business);
        if (!updated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                id, currentUser.Id, currentUser.FullName,
                "Updated Business Settings", "Business", id, null);
        }

        return NoContent();
    }
}