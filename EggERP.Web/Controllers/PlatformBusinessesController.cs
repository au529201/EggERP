
using EggERP.Domain.Entities;
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

        var adminRoleId = await _db.Roles
            .AsNoTracking()
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var businesses = await _db.Businesses
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ThenBy(b => b.Id)
            .Select(b => new
            {
                BusinessId = b.Id,
                BusinessName = b.Name,
                IsActive = b.IsActive
            })
            .ToListAsync();

        var admins = await (
            from user in _db.Users.AsNoTracking()
            join role in _db.UserRoles.AsNoTracking()
                on user.Id equals role.UserId
            where role.RoleId == adminRoleId
                  && user.BusinessId != Guid.Empty
            select new
            {
                user.BusinessId,
                user.Id,
                user.FullName,
                user.Email,
                user.EmailConfirmed,
                user.IsActive
            }
        ).ToListAsync();

        var adminsByBusiness = admins
            .GroupBy(a => a.BusinessId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(a => a.IsActive)
                    .ThenByDescending(a => a.EmailConfirmed)
                    .ThenBy(a => a.Id)
                    .First());

        var results = businesses.Select(b =>
        {
            var hasAdmin = adminsByBusiness.TryGetValue(
                b.BusinessId, out var admin);

            return new
            {
                b.BusinessId,
                b.BusinessName,
                OwnerName = hasAdmin
                    ? admin!.FullName
                    : "No administrator assigned",
                OwnerEmail = hasAdmin
                    ? admin!.Email
                    : null,
                OwnerEmailConfirmed =
                    hasAdmin && admin!.EmailConfirmed,
                b.IsActive
            };
        }).ToList();

        return Ok(results);
    }

    public sealed class ChangeBusinessStatusRequest
    {
        public bool IsActive { get; set; }
    }

    [HttpPut("{businessId:guid}/status")]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangeBusinessStatus(
        Guid businessId,
        [FromBody] ChangeBusinessStatusRequest request)
    {
        if (!Request.Headers.TryGetValue(
                "X-EzEggERP-Admin-Action",
                out var actionHeader) ||
            actionHeader.Count != 1 ||
            actionHeader[0] != "BusinessStatusChange")
        {
            return BadRequest(new
            {
                message = "Invalid administrative request."
            });
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null || !currentUser.IsActive)
            return Unauthorized();

        var business = await _db.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId);

        if (business is null)
        {
            return NotFound(new
            {
                message = "Business not found."
            });
        }

        if (business.IsActive == request.IsActive)
        {
            return Ok(new
            {
                message = "Business status is already up to date.",
                businessId,
                isActive = business.IsActive
            });
        }

        var action = request.IsActive
            ? "Reactivated Business"
            : "Suspended Business";

        var timestamp = DateTime.UtcNow;

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        business.IsActive = request.IsActive;
        business.UpdatedAtUtc = timestamp;

        _db.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            UserId = currentUser.Id,
            UserName = currentUser.FullName,
            Action = action,
            EntityType = "Business",
            EntityId = business.Id,
            Details = $"{action}: {business.Name}",
            TimestampUtc = timestamp
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = request.IsActive
                ? "Business reactivated successfully."
                : "Business suspended successfully.",
            businessId,
            isActive = business.IsActive
        });
    }
}
