using EggERP.Application.Purchases;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/purchases")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
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
    }
    [HttpPost]
    public async Task<IActionResult> CreatePurchase(CreatePurchaseRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("A purchase must have at least one item.");
        }

        var purchase = await _purchaseService.CreatePurchaseAsync(request.BusinessId, request.SupplierId, request.Items);
        return CreatedAtAction(nameof(GetPurchases), new { businessId = purchase.BusinessId }, purchase);
    }
}