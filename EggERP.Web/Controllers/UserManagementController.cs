using EggERP.Application.ActivityLogs;
using EggERP.Application.Users;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
[Authorize(Policy = "ActiveBusiness")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public UserManagementController(
        IUserManagementService userManagementService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var users = await _userManagementService.GetUsersForBusinessAsync(
            currentUser.BusinessId);

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateBusinessUserRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) =
            await _userManagementService.CreateUserAsync(request);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Created User", "User", null, $"{request.FullName} ({request.Email}) as {request.Role}");

        return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        UpdateBusinessUserRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route id does not match request id.");
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        request.BusinessId = currentUser.BusinessId;

        var (succeeded, error) =
            await _userManagementService.UpdateUserAsync(request);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Updated User", "User", id, $"{request.FullName} — role: {request.Role}");

        return NoContent();
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (id == currentUser.Id)
        {
            return BadRequest(
                "You cannot deactivate your own account.");
        }

        var targetUser = await _userManager.FindByIdAsync(
            id.ToString());

        if (targetUser is null ||
            targetUser.BusinessId != currentUser.BusinessId)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(targetUser, "Admin"))
        {
            return BadRequest(
                "Administrator accounts cannot be deactivated here.");
        }

        var (succeeded, error) =
            await _userManagementService.DeactivateUserAsync(
                currentUser.BusinessId,
                id);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Deactivated User",
            "User",
            id,
            null);

        return NoContent();
    }


    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var (succeeded, error) =
            await _userManagementService.ActivateUserAsync(
                currentUser.BusinessId,
                id);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId, currentUser.Id, currentUser.FullName,
            "Activated User", "User", id, null);

        return NoContent();
    }

    [HttpPost("{id:guid}/resend-invitation")]
    public async Task<IActionResult> ResendInvitation(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
        {
            return Unauthorized();
        }

        var (succeeded, error) =
            await _userManagementService.ResendInvitationAsync(
                currentUser.BusinessId,
                id);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Resent Employee Invitation",
            "User",
            id,
            null);

        return NoContent();
    }

}