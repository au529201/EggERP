using EggERP.Domain.Entities;
namespace EggERP.Application.Inventory;
public interface IInventoryService
{
    Task<List<EggERP.Domain.Entities.Inventory>> GetInventoryAsync(Guid businessId);
    Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(Guid businessId, Guid productId);
    Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta);
}