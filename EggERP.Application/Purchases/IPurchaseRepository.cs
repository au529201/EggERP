
using EggERP.Domain.Entities;

namespace EggERP.Application.Purchases;

public interface IPurchaseRepository
{
    Task<List<Purchase>> GetByBusinessIdAsync(Guid businessId);

    Task<Purchase?> GetByIdAsync(Guid businessId, Guid id);

    Task<List<PurchaseItem>> GetItemsByPurchaseIdAsync(
        Guid businessId,
        Guid purchaseId);

    Task<Purchase> CreateAsync(
        Purchase purchase,
        List<PurchaseItem> items);

    Task<bool> TryMarkAsPaidAsync(
        Guid businessId,
        Guid purchaseId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string? bankName);
}
