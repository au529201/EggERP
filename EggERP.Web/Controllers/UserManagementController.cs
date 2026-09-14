using EggERP.Application.Users;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementController(
        IUserManagementService userManagementService,
        UserManager<ApplicationUser> userManager)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
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

        var (succeeded, error) =
            await _userManagementService.DeactivateUserAsync(
                currentUser.BusinessId,
                id);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        return NoContent();
    }
}