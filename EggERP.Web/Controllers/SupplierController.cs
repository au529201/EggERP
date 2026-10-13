
using EggERP.Application.ActivityLogs;
using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
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

        if (user is null ||
            !user.IsActive ||
            user.BusinessId == Guid.Empty)
        {
            return null;
        }

        return user;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetSuppliers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var suppliers =
            await _supplierService.GetSuppliersAsync(user.BusinessId);

        return Ok(suppliers);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetSupplierById(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var supplier =
            await _supplierService.GetSupplierByIdAsync(
                user.BusinessId,
                id);

        if (supplier is null)
            return NotFound();

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(
        [FromBody] Supplier supplier)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest("Supplier name is required.");

        supplier.Id = Guid.NewGuid();
        supplier.BusinessId = user.BusinessId;
        supplier.Name = supplier.Name.Trim();

        var createdSupplier =
            await _supplierService.CreateSupplierAsync(supplier);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Supplier",
            "Supplier",
            createdSupplier.Id,
            createdSupplier.Name);

        return CreatedAtAction(
            nameof(GetSupplierById),
            new
            {
                businessId = user.BusinessId,
                id = createdSupplier.Id
            },
            createdSupplier);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] Supplier supplier)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (id != supplier.Id)
            return BadRequest("Supplier ID mismatch.");

        var existing =
            await _supplierService.GetSupplierByIdAsync(
                user.BusinessId,
                id);

        if (existing is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest("Supplier name is required.");

        supplier.BusinessId = user.BusinessId;
        supplier.Name = supplier.Name.Trim();

        var updated =
            await _supplierService.UpdateSupplierAsync(supplier);

        if (!updated)
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
    public async Task<IActionResult> DeactivateSupplier(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var deactivated =
            await _supplierService.DeactivateSupplierAsync(
                user.BusinessId,
                id);

        if (!deactivated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Deactivated Supplier",
            "Supplier",
            id,
            null);

        return NoContent();
    }
}
