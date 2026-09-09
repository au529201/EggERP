using EggERP.Domain.Entities;
namespace EggERP.Application.Purchases;
public interface IPurchaseRepository
{
    Task<List<Purchase>> GetByBusinessIdAsync(Guid businessId);
    Task<Purchase?> GetByIdAsync(Guid businessId, Guid id);
    Task<List<PurchaseItem>> GetItemsByPurchaseIdAsync(Guid purchaseId);
    Task<Purchase> CreateAsync(Purchase purchase, List<PurchaseItem> items);
}