
using EggERP.Application.Businesses;
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using EggERP.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IBusinessService _businessService;
    private readonly EggERPDbContext _db;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IBusinessService businessService,
        EggERPDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _businessService = businessService;
        _db = db;
    }

    private async Task<ApplicationUser?> GetAuthorizedUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return null;

        if (await _userManager.IsInRoleAsync(user, "PlatformAdmin"))
            return user;

        if (user.BusinessId == Guid.Empty)
            return null;

        var businessActive = await _db.Businesses
            .AsNoTracking()
            .AnyAsync(b =>
                b.Id == user.BusinessId &&
                b.IsActive);

        return businessActive ? user : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetAuthorizedUserAsync();

        if (user is null)
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);

        var isPlatformAdmin = roles.Contains("PlatformAdmin");

        var business = isPlatformAdmin
            ? null
            : await _businessService.GetBusinessAsync(user.BusinessId);

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            BusinessId = user.BusinessId,
            BusinessName = business?.Name ?? string.Empty,
            IsActive = user.IsActive
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateMyProfileRequest request)
    {
        var user = await GetAuthorizedUserAsync();

        if (user is null)
            return Forbid();

        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest("Full name is required.");

        if (fullName.Length > 200)
            return BadRequest(
                "Full name cannot exceed 200 characters.");

        var phoneNumber = string.IsNullOrWhiteSpace(
            request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        if (phoneNumber?.Length > 50)
            return BadRequest(
                "Phone number cannot exceed 50 characters.");

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(string.Join(
                "; ",
                result.Errors.Select(e => e.Description)));
        }

        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeMyPasswordRequest request)
    {
        var user = await GetAuthorizedUserAsync();

        if (user is null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return BadRequest("Current password is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("New password is required.");

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(
                "New password and confirmation do not match.");
        }

        if (request.CurrentPassword == request.NewPassword)
        {
            return BadRequest(
                "Your new password must be different from your current password.");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(string.Join(
                "; ",
                result.Errors.Select(e => e.Description)));
        }

        await _signInManager.RefreshSignInAsync(user);

        return NoContent();
    }
}
