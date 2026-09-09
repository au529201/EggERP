using EggERP.Domain.Entities;
namespace EggERP.Application.Sales;
public interface ISaleRepository
{
    Task<List<Sale>> GetByBusinessIdAsync(Guid businessId);
    Task<Sale?> GetByIdAsync(Guid businessId, Guid id);
    Task<List<SaleItem>> GetItemsBySaleIdAsync(Guid saleId);
    Task<Sale> CreateAsync(Sale sale, List<SaleItem> items);
}