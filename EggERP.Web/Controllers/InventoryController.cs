using EggERP.Application.Inventory;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetInventory(Guid businessId)
    {
        var inventory = await _inventoryService.GetInventoryAsync(businessId);
        return Ok(inventory);
    }
    [HttpGet("{businessId:guid}/product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(Guid businessId, Guid productId)
    {
        var record = await _inventoryService.GetByProductIdAsync(businessId, productId);
        if (record is null)
        {
            return NotFound();
        }
        return Ok(record);
    }
    public class AdjustQuantityRequest
    {
        public decimal Delta { get; set; }
    }
    [HttpPost("{businessId:guid}/product/{productId:guid}/adjust")]
    public async Task<IActionResult> AdjustQuantity(Guid businessId, Guid productId, AdjustQuantityRequest request)
    {
        var updated = await _inventoryService.AdjustQuantityAsync(businessId, productId, request.Delta);
        return Ok(updated);
    }
}