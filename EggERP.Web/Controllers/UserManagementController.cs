using EggERP.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetUsers(Guid businessId)
    {
        var users = await _userManagementService.GetUsersForBusinessAsync(businessId);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateBusinessUserRequest request)
    {
        var (succeeded, error) = await _userManagementService.CreateUserAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }
        return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateBusinessUserRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route id does not match request id.");
        }
        var (succeeded, error) = await _userManagementService.UpdateUserAsync(request);
        if (!succeeded)
        {
            return BadRequest(error);
        }
        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateUser(Guid businessId, Guid id)
    {
        var (succeeded, error) = await _userManagementService.DeactivateUserAsync(businessId, id);
        if (!succeeded)
        {
            return BadRequest(error);
        }
        return NoContent();
    }
}