using EggERP.Application.ActivityLogs;
using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
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

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetSuppliers(Guid businessId)
    {
        var suppliers = await _supplierService.GetSuppliersAsync(businessId);
        return Ok(suppliers);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetSupplierById(Guid businessId, Guid id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(businessId, id);
        if (supplier is null)
        {
            return NotFound();
        }
        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(Supplier supplier)
    {
        var createdSupplier = await _supplierService.CreateSupplierAsync(supplier);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                createdSupplier.BusinessId, currentUser.Id, currentUser.FullName,
                "Created Supplier", "Supplier", createdSupplier.Id, createdSupplier.Name);
        }

        return CreatedAtAction(
            nameof(GetSuppliers),
            new { businessId = createdSupplier.BusinessId },
            createdSupplier);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return BadRequest("Route id does not match supplier id in the request body.");
        }
        var updated = await _supplierService.UpdateSupplierAsync(supplier);
        if (!updated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                supplier.BusinessId, currentUser.Id, currentUser.FullName,
                "Updated Supplier", "Supplier", id, supplier.Name);
        }

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateSupplier(Guid businessId, Guid id)
    {
        var deactivated = await _supplierService.DeactivateSupplierAsync(businessId, id);
        if (!deactivated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                businessId, currentUser.Id, currentUser.FullName,
                "Deactivated Supplier", "Supplier", id, null);
        }

        return NoContent();
    }
}