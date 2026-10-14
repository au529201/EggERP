using EggERP.Application.Inventory;
using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using EggERP.Application.Transactions;
namespace EggERP.Application.Purchases;
public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryService _inventoryService;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITransactionManager _transactionManager;


    public PurchaseService(
        IPurchaseRepository purchaseRepository,
        IInventoryService inventoryService,
        ISupplierRepository supplierRepository,
        ITransactionManager transactionManager)
    {
        _purchaseRepository = purchaseRepository;
        _inventoryService = inventoryService;
        _supplierRepository = supplierRepository;
        _transactionManager = transactionManager;
    }

    public Task<List<Purchase>> GetPurchasesAsync(Guid businessId)
    {
        return _purchaseRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Purchase?> GetPurchaseByIdAsync(Guid businessId, Guid id)
    {
        return _purchaseRepository.GetByIdAsync(businessId, id);
    }
    public Task<List<PurchaseItem>> GetPurchaseItemsAsync(
        Guid businessId,
        Guid purchaseId)
    {
        return _purchaseRepository.GetItemsByPurchaseIdAsync(
            businessId,
            purchaseId);
    }
    public async Task<Purchase> CreatePurchaseAsync(
     Guid businessId,
     Guid? supplierId,
     List<CreatePurchaseItemRequest> items,
     string paymentMethod,
     string? paymentSource,
     string? referenceNumber,
     string? bankName,
     string status,
     DateTime? purchaseDate = null,
     string? notes = null)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);
        if (supplierId.HasValue)
        {
            var supplier = await _supplierRepository.GetByIdAsync(
                businessId,
                supplierId.Value);

            if (supplier is null)
            {
                throw new InvalidOperationException(
                    "The selected supplier does not belong to this business.");
            }
        }

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A purchase must have at least one item.");
        }

        if (businessId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid Business ID is required.");
        }

        if (status != "Paid" && status != "Pending")
        {
            throw new InvalidOperationException(
                "Status must be 'Paid' or 'Pending'.");
        }

        foreach (var item in items)
        {
            if (item.ProductId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Each purchase item must have a valid product.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase item quantity must be greater than zero.");
            }

            if (item.UnitCost < 0)
            {
                throw new InvalidOperationException(
                    "Purchase item cost cannot be negative.");
            }
        }

        var duplicateProduct = items
            .GroupBy(i => i.ProductId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateProduct is not null)
        {
            throw new InvalidOperationException(
                "The same product cannot appear more than once " +
                "in a purchase. Combine its quantities into one line.");
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
            PurchaseDateUtc = purchaseDate.HasValue
    ? DateTime.SpecifyKind(purchaseDate.Value.Date, DateTimeKind.Local).ToUniversalTime()
    : DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
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


        await _transactionManager.ExecuteAsync(async () =>
        {
            await _purchaseRepository.CreateAsync(
                purchase,
                purchaseItems);

            foreach (var item in purchaseItems)
            {
                await _inventoryService.AdjustQuantityAsync(
                    businessId,
                    item.ProductId,
                    item.Quantity);
            }
        });

        return purchase;
    }


    public async Task<(bool Succeeded, string? Error)> MarkAsPaidAsync(
        Guid businessId,
        Guid purchaseId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string? bankName)
    {
        if (businessId == Guid.Empty || purchaseId == Guid.Empty)
        {
            return (false, "A valid business and purchase are required.");
        }

        var purchase = await _purchaseRepository.GetByIdAsync(
            businessId,
            purchaseId);

        if (purchase is null)
        {
            return (false, "Purchase not found.");
        }

        if (purchase.Status == "Paid")
        {
            return (false, "This purchase is already paid.");
        }

        if (purchase.Status != "Pending")
        {
            return (false, "Only pending purchases can be marked as paid.");
        }

        try
        {
            PaymentValidation.EnsureValid(
                paymentMethod,
                referenceNumber,
                paymentSource);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        var updated = await _purchaseRepository.TryMarkAsPaidAsync(
            businessId,
            purchaseId,
            paymentMethod,
            paymentSource,
            referenceNumber,
            bankName);

        if (!updated)
        {
            return (
                false,
                "The purchase was already updated or is no longer pending. " +
                "Refresh the purchases list and try again.");
        }

        return (true, null);
    }

}