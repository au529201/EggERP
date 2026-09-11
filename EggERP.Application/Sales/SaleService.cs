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
    public async Task<Sale> CreateSaleAsync(Guid businessId, Guid? customerId, List<CreateSaleItemRequest> items, string paymentMethod, string? paymentSource, string? referenceNumber)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A sale must have at least one item.");
        }

        // Validate every line against current stock BEFORE creating anything.
        // If any single line fails, the entire sale is rejected and nothing is written.
        var stockErrors = new List<string>();

        foreach (var i in items)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(businessId, i.ProductId);
            var available = inventory?.QuantityOnHand ?? 0;

            if (i.Quantity > available)
            {
                stockErrors.Add($"only {available} in stock, but {i.Quantity} requested");
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
            Status = "Completed",
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in saleItems)
        {
            item.SaleId = sale.Id;
        }

        var createdSale = await _saleRepository.CreateAsync(sale, saleItems);

        foreach (var item in saleItems)
        {
            await _inventoryService.AdjustQuantityAsync(businessId, item.ProductId, -item.Quantity);
        }

        return createdSale;
    }
}