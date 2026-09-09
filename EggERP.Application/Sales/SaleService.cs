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
    public async Task<Sale> CreateSaleAsync(Guid businessId, Guid? customerId, List<CreateSaleItemRequest> items)
    {
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