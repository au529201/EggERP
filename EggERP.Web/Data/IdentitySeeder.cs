using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EggERP.Web.Data;

public static class IdentitySeeder
{
    // Matches the existing seeded Business row.
    private static readonly Guid DefaultBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        string[] roles = { "Admin", "Manager", "Staff" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to create role '{Role}': {Errors}", role,
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // Dev-only seed credentials. Rotate before any real deployment.
        await EnsureUserAsync(userManager, logger, "admin@egg.erp", "Admin4134", "Admin", businessId: DefaultBusinessId, fullName: "Business Admin");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string password,
        string role,
        Guid businessId,
        string fullName)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            BusinessId = businessId,
            FullName = fullName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create seed user '{Email}': {Errors}", email,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            logger.LogError("Failed to assign role '{Role}' to '{Email}': {Errors}", role, email,
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}