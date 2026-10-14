
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
[Authorize(Policy = "ActiveBusiness")]
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

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null ||
            !user.IsActive ||
            user.BusinessId == Guid.Empty)
            return null;

        return user;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetInventory(Guid businessId)
    {
        var user = await CurrentUserAsync();

        if (user is null)
            return Unauthorized();

        if (user.BusinessId != businessId)
            return Forbid();

        return Ok(await _inventoryService
            .GetInventoryAsync(user.BusinessId));
    }

    [HttpGet("{businessId:guid}/product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(
        Guid businessId, Guid productId)
    {
        var user = await CurrentUserAsync();

        if (user is null)
            return Unauthorized();

        if (user.BusinessId != businessId)
            return Forbid();

        var record = await _inventoryService.GetByProductIdAsync(
            user.BusinessId, productId);

        return record is null ? NotFound() : Ok(record);
    }


    [HttpGet("board")]
    public async Task<IActionResult> GetBoard(
        [FromQuery] DateTime asOfDate)
    {
        var user = await CurrentUserAsync();

        if (user is null)
            return Unauthorized();

        var date = asOfDate == default
            ? DateTime.Today
            : asOfDate;

        return Ok(await _inventoryService.GetBoardAsOfDateAsync(
            user.BusinessId, date));
    }

    public class ThresholdRequest
    {
        public decimal LowThreshold { get; set; }
        public decimal? HighThreshold { get; set; }
    }

    [HttpPut("product/{productId:guid}/thresholds")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetThresholds(
        Guid productId,
        [FromBody] ThresholdRequest request)
    {
        var user = await CurrentUserAsync();

        if (user is null)
            return Unauthorized();

        if (request is null)
            return BadRequest("Threshold settings are required.");

        if (request.LowThreshold < 0)
            return BadRequest(new
            {
                message = "Low threshold cannot be negative."
            });

        if (request.HighThreshold.HasValue &&
            request.HighThreshold.Value <= request.LowThreshold)
            return BadRequest(new
            {
                message = "High threshold must exceed low threshold."
            });

        try
        {
            var updated = await _inventoryService.SetThresholdsAsync(
                user.BusinessId,
                productId,
                request.LowThreshold,
                request.HighThreshold);

            await _activityLogService.LogAsync(
                user.BusinessId,
                user.Id,
                user.FullName,
                "Updated Stock Thresholds",
                "Inventory",
                productId,
                $"Low: {updated.ReorderLevel}; " +
                $"High: {updated.HighStockThreshold?.ToString() ?? "None"}");

            return Ok(new
            {
                updated.ProductId,
                updated.ReorderLevel,
                updated.HighStockThreshold
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new
            {
                message = "Product not found in this business."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
