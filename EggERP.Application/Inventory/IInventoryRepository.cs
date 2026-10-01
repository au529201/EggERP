using EggERP.Domain.Entities;

namespace EggERP.Application.Inventory;

public interface IInventoryRepository
{
    Task<List<EggERP.Domain.Entities.Inventory>> GetByBusinessIdAsync(
        Guid businessId);

    Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(
        Guid businessId,
        Guid productId);

    Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(
        Guid businessId,
        Guid productId,
        decimal delta);

    Task<EggERP.Domain.Entities.Inventory> SetReorderLevelAsync(
        Guid businessId,
        Guid productId,
        decimal reorderLevel);

    Task<Dictionary<Guid, decimal>> GetBoughtQuantitiesAsOfDateAsync(
        Guid businessId,
        DateTime asOfDate);

    Task<Dictionary<Guid, decimal>> GetSoldQuantitiesAsOfDateAsync(
        Guid businessId,
        DateTime asOfDate);
}