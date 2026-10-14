
using System.Security.Claims;
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EggERP.Web.Services;

public sealed class BusinessRevalidatingAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BusinessRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Existing Blazor circuits are rechecked every 30 seconds.
    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        ClaimsPrincipal principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated != true)
            return true;

        await using var scope = _scopeFactory.CreateAsyncScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var db = scope.ServiceProvider
            .GetRequiredService<EggERPDbContext>();

        var user = await userManager.GetUserAsync(principal);

        if (user is null || !user.IsActive)
            return false;

        if (await userManager.IsInRoleAsync(user, "PlatformAdmin"))
            return true;

        return await db.Businesses
            .AsNoTracking()
            .AnyAsync(
                b => b.Id == user.BusinessId && b.IsActive,
                cancellationToken);
    }
}
