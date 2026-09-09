using EggERP.Domain.Entities;
namespace EggERP.Application.Purchases;
public class CreatePurchaseItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
public interface IPurchaseService
{
    Task<List<Purchase>> GetPurchasesAsync(Guid businessId);
    Task<Purchase?> GetPurchaseByIdAsync(Guid businessId, Guid id);
    Task<List<PurchaseItem>> GetPurchaseItemsAsync(Guid purchaseId);
    Task<Purchase> CreatePurchaseAsync(Guid businessId, Guid? supplierId, List<CreatePurchaseItemRequest> items);
}