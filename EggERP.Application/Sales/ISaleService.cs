using EggERP.Domain.Entities;
namespace EggERP.Application.Sales;
public class CreateSaleItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SaleItemAdjustment
{
    public Guid ProductId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal SoldQuantity { get; set; }
    public bool WasDropped { get; set; }
}

public class SaleCreationResult
{
    public Sale Sale { get; set; } = null!;
    public List<SaleItemAdjustment> Adjustments { get; set; } = new();
}

public interface ISaleService
{
    Task<List<Sale>> GetSalesAsync(Guid businessId);
    Task<Sale?> GetSaleByIdAsync(Guid businessId, Guid id);
    Task<List<SaleItem>> GetSaleItemsAsync(Guid saleId);
    Task<SaleCreationResult> CreateSaleAsync(Guid businessId, Guid? customerId, List<CreateSaleItemRequest> items);
}