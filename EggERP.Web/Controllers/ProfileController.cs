using EggERP.Application.Businesses;
using EggERP.Infrastructure.Identity;
using EggERP.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IBusinessService _businessService;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IBusinessService businessService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _businessService = businessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var business =
            await _businessService.GetBusinessAsync(user.BusinessId);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            BusinessId = user.BusinessId,
            BusinessName = business?.Name ?? string.Empty,
            IsActive = user.IsActive
        };

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        UpdateMyProfileRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest("Full name is required.");
        }

        if (fullName.Length > 200)
        {
            return BadRequest(
                "Full name cannot exceed 200 characters.");
        }

        var phoneNumber = string.IsNullOrWhiteSpace(
            request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        if (phoneNumber?.Length > 50)
        {
            return BadRequest(
                "Phone number cannot exceed 50 characters.");
        }

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(
                string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description)));
        }

        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangeMyPasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return BadRequest(
                "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(
                "New password is required.");
        }

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
            return BadRequest(
                string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description)));
        }

        // Refresh the authentication cookie so the current
        // user remains signed in after changing the password.
        await _signInManager.RefreshSignInAsync(user);

        return NoContent();
    }
}