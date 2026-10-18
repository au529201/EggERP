
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

    // Revalidate existing Blazor sessions every 30 seconds.
    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        ClaimsPrincipal principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var db = scope.ServiceProvider
                .GetRequiredService<EggERPDbContext>();

            var user = await userManager.GetUserAsync(principal);

            cancellationToken.ThrowIfCancellationRequested();

            // Reject deleted or deactivated accounts.
            if (user is null || !user.IsActive)
            {
                return false;
            }

            // Platform administrators do not belong to
            // an individual business.
            if (await userManager.IsInRoleAsync(
                user,
                "PlatformAdmin"))
            {
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Regular users must belong to an active business.
            if (user.BusinessId == Guid.Empty)
            {
                return false;
            }

            var businessIsActive = await db.Businesses
                .AsNoTracking()
                .AnyAsync(
                    business =>
                        business.Id == user.BusinessId &&
                        business.IsActive,
                    cancellationToken);

            return businessIsActive;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Revalidation was canceled, commonly because
            // the Blazor circuit is closing or restarting.
            //
            // Do not interpret cancellation as evidence
            // that the account or business is inactive.
            // Existing authorization checks still apply.
            return true;
        }
    }
}
