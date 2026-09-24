using EggERP.Application.ActivityLogs;
using EggERP.Application.Purchases;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "Admin,Manager")]
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
        var purchases = await _purchaseService.GetPurchasesAsync(businessId);
        return Ok(purchases);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetPurchaseById(Guid businessId, Guid id)
    {
        var purchase = await _purchaseService.GetPurchaseByIdAsync(businessId, id);
        if (purchase is null)
        {
            return NotFound();
        }
        var items = await _purchaseService.GetPurchaseItemsAsync(id);
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
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase(CreatePurchaseRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("A purchase must have at least one item.");
        }

        try
        {
            var purchase = await _purchaseService.CreatePurchaseAsync(request.BusinessId, request.SupplierId, request.Items, request.PaymentMethod, request.PaymentSource, request.ReferenceNumber, request.BankName, request.Status);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is not null)
            {
                await _activityLogService.LogAsync(
                    request.BusinessId, currentUser.Id, currentUser.FullName,
                    "Created Purchase", "Purchase", purchase.Id,
                    $"{request.Items.Count} item(s) — Total: {purchase.TotalAmount:N2} ({request.PaymentMethod})");
            }

            return CreatedAtAction(nameof(GetPurchases), new { businessId = purchase.BusinessId }, purchase);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}