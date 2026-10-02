using EggERP.Application.Businesses;
using EggERP.Application.Email;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EggERP.Infrastructure.Businesses;

public class BusinessProvisioningService : IBusinessProvisioningService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public BusinessProvisioningService(
        IBusinessRepository businessRepository,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _businessRepository = businessRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<(bool Succeeded, Guid? BusinessId, string? Error)>
        CreateBusinessWithOwnerAsync(CreateBusinessOwnerRequest request)
    {
        var businessName = request.BusinessName?.Trim() ?? string.Empty;
        var ownerFullName = request.OwnerFullName?.Trim() ?? string.Empty;
        var ownerEmail = request.OwnerEmail?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(businessName))
        {
            return (false, null, "Business name is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerFullName))
        {
            return (false, null, "Owner name is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerEmail))
        {
            return (false, null, "Owner email is required.");
        }

        var existingUser = await _userManager.FindByEmailAsync(ownerEmail);

        if (existingUser is not null)
        {
            return (false, null,
                "A user with this email address already exists.");
        }

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = businessName,
            LegalName = Clean(request.LegalName),
            Email = Clean(request.BusinessEmail),
            Phone = Clean(request.Phone),
            AddressLine1 = Clean(request.AddressLine1),
            AddressLine2 = Clean(request.AddressLine2),
            City = Clean(request.City),
            Province = Clean(request.Province),
            PostalCode = Clean(request.PostalCode),

            CountryCode = "PH",
            TaxStatus = "Non-VAT",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _businessRepository.AddAsync(business);

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = ownerEmail,
            Email = ownerEmail,
            EmailConfirmed = false,
            BusinessId = business.Id,
            FullName = ownerFullName,
            IsActive = true
        };

        var placeholderPassword = GenerateUnusedPlaceholderPassword();

        var createUserResult =
            await _userManager.CreateAsync(owner, placeholderPassword);

        if (!createUserResult.Succeeded)
        {
            return (
                false,
                business.Id,
                string.Join(
                    "; ",
                    createUserResult.Errors.Select(e => e.Description))
            );
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var createRoleResult =
                await _roleManager.CreateAsync(
                    new IdentityRole<Guid>("Admin"));

            if (!createRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(owner);

                return (
                    false,
                    business.Id,
                    string.Join(
                        "; ",
                        createRoleResult.Errors.Select(e => e.Description))
                );
            }
        }

        var roleResult =
            await _userManager.AddToRoleAsync(owner, "Admin");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(owner);

            return (
                false,
                business.Id,
                string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description))
            );
        }

        try
        {
            await SendOwnerInviteEmailAsync(owner, business.Name);
        }
        catch (Exception ex)
        {
            return (
                false,
                business.Id,
                "The business and owner account were created, but the " +
                $"invitation email could not be sent. {ex.Message}"
            );
        }

        return (true, business.Id, null);
    }

    private async Task SendOwnerInviteEmailAsync(
        ApplicationUser owner,
        string businessName)
    {
        var token =
            await _userManager.GeneratePasswordResetTokenAsync(owner);

        var encodedToken =
            WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

        var encodedEmail =
            Uri.EscapeDataString(owner.Email!);

        var baseUrl =
            (_configuration["ApiBaseUrl"]
                ?? "https://localhost:7062/")
            .TrimEnd('/');

        var setPasswordUrl =
            $"{baseUrl}/Account/SetPassword" +
            $"?email={encodedEmail}" +
            $"&token={encodedToken}";

        var safeOwnerName =
            System.Net.WebUtility.HtmlEncode(owner.FullName);

        var safeBusinessName =
            System.Net.WebUtility.HtmlEncode(businessName);

        var subject =
            $"Welcome to EzEggERP - {businessName}";

        var html = $@"
            <p>Hi {safeOwnerName},</p>

            <p>
                Your EzEggERP business account for
                <strong>{safeBusinessName}</strong>
                has been created.
            </p>

            <p>
                You are the administrator of this business.
                Click the link below to create your password
                and activate your account.
            </p>

            <p>
                <a href='{setPasswordUrl}'>
                    Set your EzEggERP password
                </a>
            </p>

            <p>
                After activating your account, you can log in
                and manage your business, products, inventory,
                transactions, reports, managers and staff.
            </p>

            <p>
                If you were not expecting this invitation,
                you can ignore this email.
            </p>";

        await _emailSender.SendEmailAsync(
            owner.Email!,
            subject,
            html);
    }

    private static string GenerateUnusedPlaceholderPassword()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(24);

        return "Aa1" +
               Convert.ToBase64String(randomBytes)
                   .Replace("+", "A")
                   .Replace("/", "B")
                   .Replace("=", "C");
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}