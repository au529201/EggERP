using EggERP.Application.Flocks;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
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
        _productRepository = productRepository;
        _flockRepository = flockRepository;
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
    public async Task<Purchase> CreatePurchaseAsync(
        Guid businessId,
        Guid? supplierId,
        List<CreatePurchaseItemRequest> items,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string? bankName,
        string status)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A purchase must have at least one item.");
        }

        if (status != "Paid" && status != "Pending")
        {
            throw new InvalidOperationException("Status must be 'Paid' or 'Pending'.");
        }

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
            PaymentSource = paymentSource,
            ReferenceNumber = referenceNumber,
            BankName = bankName,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in purchaseItems)
        {
            item.PurchaseId = purchase.Id;
        }

        var createdPurchase = await _purchaseRepository.CreateAsync(purchase, purchaseItems);

        // This alone is the Flock/Egg "In, Bought" record now — Inventory.QuantityOnHand
        // for the purchased Product is the single source of truth, and the Flock/Egg
        // board's "Bought" column is computed live from PurchaseItems, not a separate ledger.
        foreach (var item in purchaseItems)
        {
            await _inventoryService.AdjustQuantityAsync(businessId, item.ProductId, item.Quantity);

            var product = await _productRepository.GetByIdAsync(businessId, item.ProductId);
            if (product is null)
            {
                continue;
            }

            var qty = (int)item.Quantity;

            if (product.Group == "Flock")
            {
                var flock = await _flockRepository.GetByLinkedProductIdAsync(businessId, item.ProductId);
                if (flock is not null)
                {
                    await _flockRepository.AddFlockMovementAsync(new FlockMovement
                    {
                        Id = Guid.NewGuid(),
                        FlockId = flock.Id,
                        MovementDate = purchase.PurchaseDateUtc,
                        Direction = "In",
                        Reason = "Bought",
                        Quantity = qty,
                        Notes = $"From Purchase #{purchase.Id}",
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }
            }
            else if (product.Group == "Egg")
            {
                await _flockRepository.AddEggMovementAsync(new EggMovement
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    ProductId = item.ProductId,
                    FlockId = null,
                    MovementDate = purchase.PurchaseDateUtc,
                    Direction = "In",
                    Reason = "Bought",
                    Quantity = qty,
                    Notes = $"From Purchase #{purchase.Id}",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        return createdPurchase;
    }
}