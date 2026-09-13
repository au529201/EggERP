using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/suppliers")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateSupplier(Supplier supplier)
    {
        var createdSupplier = await _supplierService.CreateSupplierAsync(supplier);
        return CreatedAtAction(
            nameof(GetSuppliers),
            new { businessId = createdSupplier.BusinessId },
            createdSupplier);
    }
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
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
        return NoContent();
    }
    [HttpDelete("{businessId:guid}/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateSupplier(Guid businessId, Guid id)
    {
        var deactivated = await _supplierService.DeactivateSupplierAsync(businessId, id);
        if (!deactivated)
        {
            return NotFound();
        }
        return NoContent();
    }
}