using EggERP.Application.Email;
using EggERP.Application.Users;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace EggERP.Infrastructure.Users;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<List<BusinessUserDto>> GetUsersForBusinessAsync(Guid businessId)
    {
        var users = _userManager.Users.Where(u => u.BusinessId == businessId).ToList();
        var result = new List<BusinessUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new BusinessUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive
            });
        }

        return result.OrderBy(u => u.FullName).ToList();
    }

    public async Task<(bool Succeeded, string? Error)> CreateUserAsync(CreateBusinessUserRequest request)
    {
        if (request.Role != "Manager" && request.Role != "Staff")
        {
            return (false, "Role must be Manager or Staff.");
        }

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return (false, "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false,
            BusinessId = request.BusinessId,
            FullName = request.FullName,
            IsActive = true
        };

        // Identity's CreateAsync requires a password that satisfies the
        // configured policy, but this one is never communicated to the user
        // and never used to log in: they set their own password via the
        // invite link below, which overwrites this value entirely.
        var placeholderPassword = GenerateUnusedPlaceholderPassword();

        var createResult = await _userManager.CreateAsync(user, placeholderPassword);
        if (!createResult.Succeeded)
        {
            return (false, string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            return (false, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        await SendInviteEmailAsync(user);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateUserAsync(UpdateBusinessUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null || user.BusinessId != request.BusinessId)
        {
            return (false, "User not found in this business.");
        }

        user.FullName = request.FullName;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (false, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(request.Role))
        {
            if (request.Role != "Manager" && request.Role != "Staff")
            {
                return (false, "Role must be Manager or Staff.");
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addRoleResult.Succeeded)
            {
                return (false, string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeactivateUserAsync(Guid businessId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.BusinessId != businessId)
        {
            return (false, "User not found in this business.");
        }

        user.IsActive = false;
        user.LockoutEnd = DateTimeOffset.MaxValue; // prevents login even if IsActive check is bypassed elsewhere
        user.LockoutEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }
    public async Task<(bool Succeeded, string? Error)> ActivateUserAsync(Guid businessId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.BusinessId != businessId)
        {
            return (false, "User not found in this business.");
        }

        user.IsActive = true;
        user.LockoutEnd = null;
        user.LockoutEnabled = false;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    private async Task SendInviteEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
        var encodedEmail = Uri.EscapeDataString(user.Email!);

        var baseUrl = (_configuration["ApiBaseUrl"] ?? "https://localhost:7062/").TrimEnd('/');
        var setPasswordUrl = $"{baseUrl}/Account/SetPassword?email={encodedEmail}&token={encodedToken}";

        var subject = "You've been invited to EggERP";
        var html = $@"
            <p>Hi {user.FullName},</p>
            <p>You've been invited to EggERP. Click the link below to set your password and activate your account:</p>
            <p><a href='{setPasswordUrl}'>Set your password</a></p>
            <p>If you weren't expecting this invitation, you can ignore this email.</p>";

        await _emailSender.SendEmailAsync(user.Email!, subject, html);
    }

    private static string GenerateUnusedPlaceholderPassword()
    {
        // Satisfies Identity's password policy (upper, lower, digit, 8+ chars)
        // but is never shown to anyone and never used to sign in.
        var randomBytes = RandomNumberGenerator.GetBytes(24);
        return "Aa1" + Convert.ToBase64String(randomBytes).Replace("+", "A").Replace("/", "B").Replace("=", "C");
    }
}