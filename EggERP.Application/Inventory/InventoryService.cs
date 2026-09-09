using EggERP.Domain.Entities;
namespace EggERP.Application.Inventory;
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }
    public Task<List<EggERP.Domain.Entities.Inventory>> GetInventoryAsync(Guid businessId)
    {
        return _inventoryRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(Guid businessId, Guid productId)
    {
        return _inventoryRepository.GetByProductIdAsync(businessId, productId);
    }
    public Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta)
    {
        return _inventoryRepository.AdjustQuantityAsync(businessId, productId, delta);
    }
}