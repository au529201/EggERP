using EggERP.Application.Inventory;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Inventory;
public class InventoryRepository : IInventoryRepository
{
    private readonly EggERPDbContext _dbContext;
    public InventoryRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<EggERP.Domain.Entities.Inventory>> GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Inventories
            .Where(i => i.BusinessId == businessId)
            .ToListAsync();
    }
    public Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(Guid businessId, Guid productId)
    {
        return _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.BusinessId == businessId && i.ProductId == productId);
    }
    public async Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta)
    {
        var existing = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.BusinessId == businessId && i.ProductId == productId);

        if (existing is null)
        {
            var newRecord = new EggERP.Domain.Entities.Inventory
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                ProductId = productId,
                QuantityOnHand = Math.Max(0, delta),
                ReorderLevel = 0,
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.Inventories.Add(newRecord);
            await _dbContext.SaveChangesAsync();
            return newRecord;
        }

        existing.QuantityOnHand = Math.Max(0, existing.QuantityOnHand + delta);
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return existing;
    }
}