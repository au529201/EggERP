
using EggERP.Domain.Entities;

namespace EggERP.Application.Sales;

public interface ISaleRepository
{
    Task<List<Sale>> GetByBusinessIdAsync(Guid businessId);

    Task<Sale?> GetByIdAsync(Guid businessId, Guid id);

    Task<List<SaleItem>> GetItemsBySaleIdAsync(
        Guid businessId,
        Guid saleId);

    Task<Sale> CreateAsync(
        Sale sale,
        List<SaleItem> items);

    Task<bool> TryMarkAsPaidAsync(
        Guid businessId,
        Guid saleId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber);
}
