using EggERP.Application.Inventory;
using EggERP.Domain.Entities;
namespace EggERP.Application.Sales;
public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryService _inventoryService;

    public SaleService(ISaleRepository saleRepository, IInventoryService inventoryService)
    {
        _saleRepository = saleRepository;
        _inventoryService = inventoryService;
    }
    public Task<List<Sale>> GetSalesAsync(Guid businessId)
    {
        return _saleRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Sale?> GetSaleByIdAsync(Guid businessId, Guid id)
    {
        return _saleRepository.GetByIdAsync(businessId, id);
    }
    public Task<List<SaleItem>> GetSaleItemsAsync(Guid saleId)
    {
        return _saleRepository.GetItemsBySaleIdAsync(saleId);
    }
    public async Task<Sale> CreateSaleAsync(
        Guid businessId,
        Guid? customerId,
        List<CreateSaleItemRequest> items,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string status)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A sale must have at least one item.");
        }

        if (status != "Paid" && status != "Pending")
        {
            throw new InvalidOperationException("Status must be 'Paid' or 'Pending'.");
        }

        var stockErrors = new List<string>();

        foreach (var i in items)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(businessId, i.ProductId);
            var available = inventory?.QuantityOnHand ?? 0;

            if (i.Quantity > available)
            {
                stockErrors.Add($"only {(int)available} in stock, but {(int)i.Quantity} requested");
            }
        }

        if (stockErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot complete sale: {string.Join("; ", stockErrors)}. Adjust the quantity or remove the affected item(s) before continuing.");
        }

        var saleItems = items.Select(i => new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountAmount = 0,
            TaxAmount = 0,
            TotalAmount = i.Quantity * i.UnitPrice
        }).ToList();

        var subtotal = saleItems.Sum(si => si.TotalAmount);

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CustomerId = customerId,
            SaleDateUtc = DateTime.UtcNow,
            Subtotal = subtotal,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = subtotal,
            PaymentMethod = paymentMethod,
            PaymentSource = paymentSource,
            ReferenceNumber = referenceNumber,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in saleItems)
        {
            item.SaleId = sale.Id;
        }

        var createdSale = await _saleRepository.CreateAsync(sale, saleItems);

        // This alone is the Flock/Egg "Out, Sold" record now — see PurchaseService for
        // the matching note on the Bought side.
        foreach (var item in saleItems)
        {
            await _inventoryService.AdjustQuantityAsync(businessId, item.ProductId, -item.Quantity);
        }

        return createdSale;
    }

    public async Task<(bool Succeeded, string? Error)> MarkAsPaidAsync(
        Guid businessId,
        Guid saleId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber)
    {
        var sale = await _saleRepository.GetByIdAsync(
            businessId,
            saleId);

        if (sale is null)
        {
            return (false, "Sale not found.");
        }

        if (sale.Status == "Paid")
        {
            return (false, "This sale is already paid.");
        }

        if (sale.Status != "Pending")
        {
            return (false, "Only pending sales can be marked as paid.");
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

        sale.Status = "Paid";
        sale.PaymentMethod = paymentMethod;
        sale.PaymentSource = paymentSource;
        sale.ReferenceNumber = referenceNumber;
        sale.UpdatedAtUtc = DateTime.UtcNow;

        await _saleRepository.UpdateAsync(sale);

        return (true, null);
    }
}