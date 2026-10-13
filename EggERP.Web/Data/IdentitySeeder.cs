
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EggERP.Web.Data;

public static class IdentitySeeder
{
    private static readonly Guid DefaultBusinessId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<
            RoleManager<IdentityRole<Guid>>>();

        var userManager = services.GetRequiredService<
            UserManager<ApplicationUser>>();

        var configuration = services.GetRequiredService<
            IConfiguration>();

        var logger = services.GetRequiredService<
            ILoggerFactory>().CreateLogger("IdentitySeeder");

        string[] roles =
        {
            "PlatformAdmin",
            "Admin",
            "Manager",
            "Staff"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<Guid>(role));

                if (!result.Succeeded)
                {
                    logger.LogError(
                        "Unable to create role {Role}: {Errors}",
                        role,
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));

                    throw new InvalidOperationException(
                        $"Unable to initialize role {role}.");
                }
            }
        }

        // Existing default Business Admin.
        await SeedAccountAsync(
            userManager,
            logger,
            configuration["Seed:AdminEmail"],
            configuration["Seed:AdminPassword"],
            configuration["Seed:AdminFullName"],
            "Admin",
            DefaultBusinessId);

        // Platform-level Administrator.
        // This account is not assigned to a business.
        await SeedAccountAsync(
            userManager,
            logger,
            configuration["PlatformAdmin:Email"],
            configuration["PlatformAdmin:Password"],
            configuration["PlatformAdmin:FullName"],
            "PlatformAdmin",
            Guid.Empty);
    }

    private static async Task SeedAccountAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string? email,
        string? password,
        string? fullName,
        string role,
        Guid businessId)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation(
                "Seeding skipped for {Role}: credentials not configured.",
                role);
            return;
        }

        email = email.Trim().ToLowerInvariant();

        var existing = await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            // Never silently grant PlatformAdmin to an
            // existing business user.
            if (!await userManager.IsInRoleAsync(existing, role))
            {
                logger.LogWarning(
                    "Account {Email} exists but is not assigned {Role}. " +
                    "No automatic role change was performed.",
                    email, role);
            }

            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            BusinessId = businessId,
            FullName = string.IsNullOrWhiteSpace(fullName)
                ? email
                : fullName.Trim(),
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            logger.LogError(
                "Unable to seed {Role}: {Errors}",
                role,
                string.Join("; ",
                    result.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "Unable to assign {Role}: {Errors}",
                role,
                string.Join("; ",
                    roleResult.Errors.Select(e => e.Description)));

            var cleanup = await userManager.DeleteAsync(user);

            if (!cleanup.Succeeded)
            {
                logger.LogError(
                    "Unable to clean up account after role failure.");
            }
        }
    }
}
