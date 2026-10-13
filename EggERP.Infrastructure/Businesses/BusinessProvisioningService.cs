
using EggERP.Application.Businesses;
using EggERP.Application.Email;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace EggERP.Infrastructure.Businesses;

public class BusinessProvisioningService
    : IBusinessProvisioningService
{
    private readonly EggERPDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public BusinessProvisioningService(
        EggERPDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<(bool Succeeded, Guid? BusinessId, string? Error)>
        CreateBusinessWithOwnerAsync(
            CreateBusinessOwnerRequest request)
    {
        var businessName = request.BusinessName?.Trim() ?? "";
        var ownerName = request.OwnerFullName?.Trim() ?? "";
        var ownerEmail = request.OwnerEmail?
            .Trim().ToLowerInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(businessName) ||
            businessName.Length > 200)
        {
            return (false, null,
                "A valid business name is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerName) ||
            ownerName.Length > 100)
        {
            return (false, null,
                "A valid owner name is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerEmail) ||
            !new System.ComponentModel.DataAnnotations
                .EmailAddressAttribute().IsValid(ownerEmail) ||
            ownerEmail.Length > 256)
        {
            return (false, null,
                "A valid owner email is required.");
        }

        if (await _userManager.FindByEmailAsync(ownerEmail)
            is not null)
        {
            return (false, null,
                "An account with this email already exists.");
        }

        // Identity roles should be seeded before provisioning.
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            return (false, null,
                "Admin role is not configured.");
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

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = ownerEmail,
            Email = ownerEmail,
            EmailConfirmed = false,
            BusinessId = business.Id,
            FullName = ownerName,
            IsActive = true
        };

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Businesses.Add(business);
            await _db.SaveChangesAsync();

            var temporaryPassword =
                GenerateUnusedPlaceholderPassword();

            var createResult = await _userManager.CreateAsync(
                owner, temporaryPassword);

            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, null, Errors(createResult));
            }

            var roleResult = await _userManager.AddToRoleAsync(
                owner, "Admin");

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, null, Errors(roleResult));
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, null,
                "Business creation failed. Please try again.");
        }

        // Email delivery occurs after the database commit.
        // If sending fails, the pending account is retained.
        try
        {
            await SendOwnerInviteEmailAsync(
                owner, business.Name);
        }
        catch
        {
            return (false, business.Id,
                "The business and owner account were created, " +
                "but the invitation email could not be sent. " +
                "Do not create the business again. " +
                "The invitation needs to be resent.");
        }

        return (true, business.Id, null);
    }

    private async Task SendOwnerInviteEmailAsync(
        ApplicationUser owner,
        string businessName)
    {
        var token = await _userManager
            .GeneratePasswordResetTokenAsync(owner);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

        var encodedEmail = Uri.EscapeDataString(
            owner.Email!);

        var baseUrl = (
            _configuration["ApiBaseUrl"]
            ?? "https://localhost:7062/"
        ).TrimEnd('/');

        var url = $"{baseUrl}/Account/SetPassword" +
                  $"?email={encodedEmail}&token={encodedToken}";

        var safeName = HtmlEncoder.Default.Encode(
            owner.FullName);

        var safeBusiness = HtmlEncoder.Default.Encode(
            businessName);

        var safeUrl = HtmlEncoder.Default.Encode(url);

        var subject =
            "Welcome to EzEggERP - Business Owner Invitation";

        var html = $@"
            <h2>Welcome to EzEggERP!</h2>

            <p>Hello {safeName},</p>

            <p>
                You have been invited to manage
                <strong>{safeBusiness}</strong>.
            </p>

            <p>
                Your account has been created as
                the Business Administrator.
            </p>

            <p>
                <a href=""{safeUrl}"">
                    Set Your Password
                </a>
            </p>

            <p>
                After activating your account,
                you can manage your business settings,
                inventory, transactions and employees.
            </p>

            <p>
                If you did not expect this invitation,
                you can ignore this email.
            </p>";

        await _emailSender.SendEmailAsync(
            owner.Email!,
            subject,
            html);
    }

    private static string GenerateUnusedPlaceholderPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);

        return "Aa1" +
            Convert.ToBase64String(bytes)
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

    private static string Errors(IdentityResult result)
    {
        return string.Join("; ",
            result.Errors.Select(e => e.Description));
    }
}
