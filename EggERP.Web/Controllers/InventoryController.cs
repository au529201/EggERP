using EggERP.Application.ActivityLogs;
using EggERP.Application.Inventory;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public InventoryController(
        IInventoryService inventoryService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _inventoryService = inventoryService;
        _userManager = userManager;
        _activityLogService = activityLogService;
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

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                businessId, currentUser.Id, currentUser.FullName,
                "Adjusted Inventory", "Inventory", productId,
                $"Delta: {request.Delta:+0.##;-0.##;0} — New quantity: {updated.QuantityOnHand}");
        }

        return Ok(updated);
    }
}