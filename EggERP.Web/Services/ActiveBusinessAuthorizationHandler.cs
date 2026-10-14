
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Web.Services;

public sealed class ActiveBusinessAuthorizationHandler
    : AuthorizationHandler<ActiveBusinessRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EggERPDbContext _db;

    public ActiveBusinessAuthorizationHandler(
        UserManager<ApplicationUser> userManager,
        EggERPDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveBusinessRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var user = await _userManager.GetUserAsync(context.User);

        if (user is null ||
            !user.IsActive ||
            user.BusinessId == Guid.Empty)
        {
            return;
        }

        var businessActive = await _db.Businesses
            .AsNoTracking()
            .AnyAsync(b =>
                b.Id == user.BusinessId &&
                b.IsActive);

        if (businessActive)
            context.Succeed(requirement);
    }
}
