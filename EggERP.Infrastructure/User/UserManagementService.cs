
using EggERP.Application.Email;
using EggERP.Application.Users;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

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

    // ==========================================
    // GET USERS FOR CURRENT BUSINESS
    // ==========================================
    public async Task<List<BusinessUserDto>> GetUsersForBusinessAsync(
        Guid businessId)
    {
        var users = _userManager.Users
            .Where(u => u.BusinessId == businessId)
            .ToList();

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
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        return result
            .OrderBy(u => u.FullName)
            .ToList();
    }

    // ==========================================
    // CREATE USER AND SEND INVITATION
    // ==========================================
    public async Task<(bool Succeeded, string? Error)> CreateUserAsync(
        CreateBusinessUserRequest request)
    {
        if (request.Role != "Manager" &&
            request.Role != "Staff")
        {
            return (false, "Role must be Manager or Staff.");
        }

        if (request.BusinessId == Guid.Empty)
        {
            return (false, "Invalid business.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return (false, "Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return (false, "Email is required.");
        }

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();

        if (fullName.Length > 200)
        {
            return (false,
                "Full name cannot exceed 200 characters.");
        }

        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return (false,
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            BusinessId = request.BusinessId,
            FullName = fullName,
            IsActive = true
        };

        // Temporary random password.
        // Employee chooses their own password
        // through the emailed invitation.
        var placeholderPassword =
            GenerateUnusedPlaceholderPassword();

        var createResult = await _userManager.CreateAsync(
            user,
            placeholderPassword);

        if (!createResult.Succeeded)
        {
            return (false, FormatErrors(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            request.Role);

        if (!roleResult.Succeeded)
        {
            // Clean up the newly created account
            // if role assignment fails.
            await _userManager.DeleteAsync(user);

            return (false, FormatErrors(roleResult));
        }

        try
        {
            await SendInviteEmailAsync(user);
        }
        catch
        {
            // Keep the pending user so an invitation
            // can be resent when that feature is added.
            return (false,
                "User was created, but the invitation email " +
                "could not be sent. Please check email settings.");
        }

        return (true, null);
    }

    // ==========================================
    // UPDATE USER INFORMATION AND ROLE
    // ==========================================
    public async Task<(bool Succeeded, string? Error)> UpdateUserAsync(
        UpdateBusinessUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(
            request.Id.ToString());

        if (user is null ||
            user.BusinessId != request.BusinessId)
        {
            return (false,
                "User not found in this business.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return (false, "Full name is required.");
        }

        var fullName = request.FullName.Trim();

        if (fullName.Length > 200)
        {
            return (false,
                "Full name cannot exceed 200 characters.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var isAdmin = currentRoles.Contains("Admin");

        // Protect existing Admin accounts.
        if (isAdmin)
        {
            if (request.Role != "Admin")
            {
                return (false,
                    "Administrator roles cannot be changed here.");
            }
        }
        else
        {
            // Only Manager and Staff are assignable.
            if (request.Role != "Manager" &&
                request.Role != "Staff")
            {
                return (false,
                    "Role must be Manager or Staff.");
            }
        }

        // Preserve the original name in case
        // role assignment fails.
        var originalFullName = user.FullName;

        user.FullName = fullName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return (false, FormatErrors(updateResult));
        }

        // Admin role cannot be modified.
        if (isAdmin)
        {
            return (true, null);
        }

        // Change role only when necessary.
        if (!currentRoles.Contains(request.Role))
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                user.FullName = originalFullName;
                await _userManager.UpdateAsync(user);

                return (false, FormatErrors(removeResult));
            }

            var addResult =
                await _userManager.AddToRoleAsync(
                    user,
                    request.Role);

            if (!addResult.Succeeded)
            {
                // Best-effort recovery.
                await _userManager.AddToRolesAsync(
                    user,
                    currentRoles);

                user.FullName = originalFullName;
                await _userManager.UpdateAsync(user);

                return (false, FormatErrors(addResult));
            }
        }

        return (true, null);
    }

    // ==========================================
    // DEACTIVATE USER
    // ==========================================
    public async Task<(bool Succeeded, string? Error)> DeactivateUserAsync(
        Guid businessId,
        Guid userId)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null ||
            user.BusinessId != businessId)
        {
            return (false,
                "User not found in this business.");
        }

        // Never deactivate Admin accounts
        // through employee management.
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return (false,
                "Administrator accounts cannot be deactivated.");
        }

        if (!user.IsActive)
        {
            return (false,
                "This user is already deactivated.");
        }

        user.IsActive = false;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return (false, FormatErrors(result));
        }

        return (true, null);
    }

    // ==========================================
    // ACTIVATE USER
    // ==========================================
    public async Task<(bool Succeeded, string? Error)> ActivateUserAsync(
        Guid businessId,
        Guid userId)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null ||
            user.BusinessId != businessId)
        {
            return (false,
                "User not found in this business.");
        }

        if (user.IsActive)
        {
            return (false,
                "This user is already active.");
        }

        user.IsActive = true;
        user.LockoutEnd = null;
        user.LockoutEnabled = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return (false, FormatErrors(result));
        }

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)>
        ResendInvitationAsync(Guid businessId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null || user.BusinessId != businessId)
        {
            return (false, "User not found in this business.");
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return (false,
                "Administrator invitations cannot be resent here.");
        }

        if (!await _userManager.IsInRoleAsync(user, "Manager") &&
            !await _userManager.IsInRoleAsync(user, "Staff"))
        {
            return (false, "Only employees can receive invitations.");
        }

        if (!user.IsActive)
        {
            return (false,
                "Activate this employee before resending an invitation.");
        }

        if (user.EmailConfirmed)
        {
            return (false,
                "This employee has already activated their account.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return (false, "This employee has no valid email address.");
        }

        try
        {
            await SendInviteEmailAsync(user);
            return (true, null);
        }
        catch
        {
            return (false,
                "The invitation email could not be sent. Please try again.");
        }
    }


    // ==========================================
    // SEND BREVO INVITATION EMAIL
    // ==========================================
    private async Task SendInviteEmailAsync(ApplicationUser user)

    {
        var token =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

        var encodedEmail = Uri.EscapeDataString(user.Email!);

        var baseUrl =
            (_configuration["ApiBaseUrl"] ??
             "https://localhost:7062/").TrimEnd('/');

        var setPasswordUrl =
            $"{baseUrl}/Account/SetPassword" +
            $"?email={encodedEmail}&token={encodedToken}";

        var safeName = HtmlEncoder.Default.Encode(user.FullName);
        var safeUrl = HtmlEncoder.Default.Encode(setPasswordUrl);

        var subject = "You've been invited to EzEggERP";

        var html = $@"
            <h2>Welcome to EzEggERP!</h2>

            <p>Hello {safeName},</p>

            <p>
                You have been invited to join EzEggERP.
                Please click the link below to set
                your password and activate your account.
            </p>

            <p>
                <a href=""{safeUrl}"">
                    Set Your Password
                </a>
            </p>

            <p>
                If you did not expect this invitation,
                you can safely ignore this email.
            </p>";

        await _emailSender.SendEmailAsync(
            user.Email!,
            subject,
            html);
    }

    // ==========================================
    // GENERATE SECURE TEMPORARY PASSWORD
    // ==========================================
    private static string GenerateUnusedPlaceholderPassword()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(24);

        return "Aa1" +
            Convert.ToBase64String(randomBytes)
                .Replace("+", "A")
                .Replace("/", "B")
                .Replace("=", "C");
    }

    // ==========================================
    // FORMAT IDENTITY ERRORS
    // ==========================================
    private static string FormatErrors(IdentityResult result)
    {
        return string.Join(
            "; ",
            result.Errors.Select(e => e.Description));
    }
}
