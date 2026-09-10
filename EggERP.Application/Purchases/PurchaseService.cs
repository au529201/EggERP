using EggERP.Application.Inventory;
using EggERP.Domain.Entities;
namespace EggERP.Application.Purchases;
public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryService _inventoryService;
    public PurchaseService(IPurchaseRepository purchaseRepository, IInventoryService inventoryService)
    {
        _purchaseRepository = purchaseRepository;
        _inventoryService = inventoryService;
    }
    public Task<List<Purchase>> GetPurchasesAsync(Guid businessId)
    {
        return _purchaseRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Purchase?> GetPurchaseByIdAsync(Guid businessId, Guid id)
    {
        return _purchaseRepository.GetByIdAsync(businessId, id);
    }
    public Task<List<PurchaseItem>> GetPurchaseItemsAsync(Guid purchaseId)
    {
        return _purchaseRepository.GetItemsByPurchaseIdAsync(purchaseId);
    }
    public async Task<Purchase> CreatePurchaseAsync(Guid businessId, Guid? supplierId, List<CreatePurchaseItemRequest> items, string paymentMethod, string? referenceNumber)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber);

        var purchaseItems = items.Select(i => new PurchaseItem
        {
            Id = Guid.NewGuid(),
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitCost = i.UnitCost,
            DiscountAmount = 0,
            TaxAmount = 0,
            TotalAmount = i.Quantity * i.UnitCost
        }).ToList();

        var subtotal = purchaseItems.Sum(pi => pi.TotalAmount);

        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            SupplierId = supplierId,
            PurchaseDateUtc = DateTime.UtcNow,
            Subtotal = subtotal,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = subtotal,
            PaymentMethod = paymentMethod,
            ReferenceNumber = referenceNumber,
            Status = "Completed",
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in purchaseItems)
        {
            item.PurchaseId = purchase.Id;
        }

        var createdPurchase = await _purchaseRepository.CreateAsync(purchase, purchaseItems);

        foreach (var item in purchaseItems)
        {
            await _inventoryService.AdjustQuantityAsync(businessId, item.ProductId, item.Quantity);
        }

        return createdPurchase;
    }
}