
using EggERP.Application.ActivityLogs;
using EggERP.Application.Purchases;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "Admin,Manager,Staff")]
[Authorize(Policy = "ActiveBusiness")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public PurchaseController(
        IPurchaseService purchaseService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _purchaseService = purchaseService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetPurchases(Guid businessId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        return Ok(await _purchaseService.GetPurchasesAsync(businessId));
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetPurchaseById(
        Guid businessId, Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var purchase = await _purchaseService.GetPurchaseByIdAsync(
            businessId, id);

        if (purchase is null)
            return NotFound();

        var items = await _purchaseService.GetPurchaseItemsAsync(
            businessId, id);

        return Ok(new { purchase, items });
    }

    public class CreatePurchaseRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? SupplierId { get; set; }
        public List<CreatePurchaseItemRequest> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? BankName { get; set; }
        public string Status { get; set; } = "Paid";
        public DateTime? PurchaseDate { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase(
        CreatePurchaseRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != request.BusinessId)
            return Forbid();

        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("A purchase must have at least one item.");

        try
        {
            var purchase = await _purchaseService.CreatePurchaseAsync(
                user.BusinessId,
                request.SupplierId,
                request.Items,
                request.PaymentMethod,
                request.PaymentSource,
                request.ReferenceNumber,
                request.BankName,
                request.Status,
                request.PurchaseDate,
                request.Notes);

            await _activityLogService.LogAsync(
                user.BusinessId, user.Id, user.FullName,
                "Created Purchase", "Purchase", purchase.Id,
                $"{request.Items.Count} item(s) — Total: " +
                $"{purchase.TotalAmount:N2} ({request.PaymentMethod})");

            return CreatedAtAction(
                nameof(GetPurchases),
                new { businessId = user.BusinessId },
                purchase);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public class MarkPurchasePaidRequest
    {
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? BankName { get; set; }
    }

    [HttpPut("{businessId:guid}/{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(
        Guid businessId, Guid id,
        MarkPurchasePaidRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var (succeeded, error) =
            await _purchaseService.MarkAsPaidAsync(
                businessId, id, request.PaymentMethod,
                request.PaymentSource, request.ReferenceNumber,
                request.BankName);

        if (!succeeded)
            return BadRequest(error);

        await _activityLogService.LogAsync(
            businessId, user.Id, user.FullName,
            "Purchase Payment Completed", "Purchase", id,
            $"Marked as Paid ({request.PaymentMethod})");

        return NoContent();
    }

    [HttpPut("{businessId:guid}/{id:guid}/mark-pending")]
    public async Task<IActionResult> MarkAsPending(
        Guid businessId, Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var (succeeded, error) =
            await _purchaseService.MarkAsPendingAsync(businessId, id);

        if (!succeeded)
            return BadRequest(error);

        await _activityLogService.LogAsync(
            businessId, user.Id, user.FullName,
            "Purchase Marked Pending", "Purchase", id,
            "Payment status changed from Paid to Pending.");

        return NoContent();
    }
}
