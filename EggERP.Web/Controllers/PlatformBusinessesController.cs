
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/provisioning/businesses")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformBusinessesController : ControllerBase
{
    private readonly EggERPDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlatformBusinessesController(
        EggERPDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetBusinesses()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
            return Unauthorized();

        var adminRole = await _db.Roles
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var results = await (
            from business in _db.Businesses
            join owner in _db.Users
                on business.Id equals owner.BusinessId
            join userRole in _db.UserRoles
                on owner.Id equals userRole.UserId
            where userRole.RoleId == adminRole
            select new
            {
                BusinessId = business.Id,
                BusinessName = business.Name,
                OwnerName = owner.FullName,
                OwnerEmail = owner.Email,
                OwnerEmailConfirmed = owner.EmailConfirmed
            }
        ).ToListAsync();

        return Ok(results);
    }
}
