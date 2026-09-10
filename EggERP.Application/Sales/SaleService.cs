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
    public async Task<SaleCreationResult> CreateSaleAsync(Guid businessId, Guid? customerId, List<CreateSaleItemRequest> items)
    {
        var saleItems = new List<SaleItem>();
        var adjustments = new List<SaleItemAdjustment>();

        foreach (var i in items)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(businessId, i.ProductId);
            var available = inventory?.QuantityOnHand ?? 0;

            if (available <= 0)
            {
                adjustments.Add(new SaleItemAdjustment
                {
                    ProductId = i.ProductId,
                    RequestedQuantity = i.Quantity,
                    SoldQuantity = 0,
                    WasDropped = true
                });
                continue;
            }

            var quantityToSell = Math.Min(i.Quantity, available);

            if (quantityToSell < i.Quantity)
            {
                adjustments.Add(new SaleItemAdjustment
                {
                    ProductId = i.ProductId,
                    RequestedQuantity = i.Quantity,
                    SoldQuantity = quantityToSell,
                    WasDropped = false
                });
            }

            saleItems.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = quantityToSell,
                UnitPrice = i.UnitPrice,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = quantityToSell * i.UnitPrice
            });
        }

        if (saleItems.Count == 0)
        {
            throw new InvalidOperationException("No items could be sold: all requested products are out of stock.");
        }

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

        return new SaleCreationResult
        {
            Sale = createdSale,
            Adjustments = adjustments
        };
    }
}