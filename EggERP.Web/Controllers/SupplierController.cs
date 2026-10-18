
using EggERP.Application.ActivityLogs;
using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,Staff")]
[Authorize(Policy = "ActiveBusiness")]
[Route("api/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public SupplierController(
        ISupplierService supplierService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _supplierService = supplierService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    private async Task<ApplicationUser?> GetActiveUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return user is not null &&
               user.IsActive &&
               user.BusinessId != Guid.Empty
            ? user
            : null;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetSuppliers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        return Ok(await _supplierService.GetSuppliersAsync(businessId));
    }

    [HttpGet("{businessId:guid}/all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllSuppliers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        return Ok(await _supplierService.GetAllSuppliersAsync(businessId));
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetSupplierById(Guid businessId, Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var supplier = await _supplierService
            .GetSupplierByIdAsync(businessId, id);

        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(
        [FromBody] Supplier supplier)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (supplier is null ||
            string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest("Supplier name is required.");

        supplier.Id = Guid.NewGuid();
        supplier.BusinessId = user.BusinessId;
        supplier.Name = supplier.Name.Trim();

        var created = await _supplierService.CreateSupplierAsync(supplier);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Supplier",
            "Supplier",
            created.Id,
            created.Name);

        return CreatedAtAction(
            nameof(GetSupplierById),
            new { businessId = user.BusinessId, id = created.Id },
            created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] Supplier supplier)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (supplier is null || id != supplier.Id)
            return BadRequest("Supplier ID mismatch.");

        if (string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest("Supplier name is required.");

        var existing = await _supplierService
            .GetSupplierByIdAsync(user.BusinessId, id);

        if (existing is null)
            return NotFound();

        supplier.BusinessId = user.BusinessId;
        supplier.Name = supplier.Name.Trim();

        if (!await _supplierService.UpdateSupplierAsync(supplier))
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Updated Supplier",
            "Supplier",
            id,
            supplier.Name);

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateSupplier(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        if (!await _supplierService.DeactivateSupplierAsync(businessId, id))
            return NotFound();

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Deactivated Supplier",
            "Supplier",
            id,
            null);

        return NoContent();
    }

    [HttpPatch("{businessId:guid}/{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ReactivateSupplier(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        if (!await _supplierService.ReactivateSupplierAsync(businessId, id))
            return NotFound();

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Reactivated Supplier",
            "Supplier",
            id,
            null);

        return NoContent();
    }
}
