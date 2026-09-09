using EggERP.Domain.Entities;
namespace EggERP.Application.Sales;
public class CreateSaleItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
public interface ISaleService
{
    Task<List<Sale>> GetSalesAsync(Guid businessId);
    Task<Sale?> GetSaleByIdAsync(Guid businessId, Guid id);
    Task<List<SaleItem>> GetSaleItemsAsync(Guid saleId);
    Task<Sale> CreateSaleAsync(Guid businessId, Guid? customerId, List<CreateSaleItemRequest> items);
}