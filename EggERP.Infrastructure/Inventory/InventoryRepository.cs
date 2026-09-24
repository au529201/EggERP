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

    public async Task<Dictionary<Guid, decimal>> GetBoughtQuantitiesAsOfDateAsync(Guid businessId, DateTime asOfDate)
    {
        var endOfDay = asOfDate.Date.AddDays(1);

        var query =
            from pi in _dbContext.PurchaseItems
            join p in _dbContext.Purchases on pi.PurchaseId equals p.Id
            where p.BusinessId == businessId && p.PurchaseDateUtc < endOfDay
            group pi by pi.ProductId into g
            select new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) };

        return await query.ToDictionaryAsync(x => x.ProductId, x => x.Total);
    }

    public async Task<Dictionary<Guid, decimal>> GetSoldQuantitiesAsOfDateAsync(Guid businessId, DateTime asOfDate)
    {
        var endOfDay = asOfDate.Date.AddDays(1);

        var query =
            from si in _dbContext.SaleItems
            join s in _dbContext.Sales on si.SaleId equals s.Id
            where s.BusinessId == businessId && s.SaleDateUtc < endOfDay
            group si by si.ProductId into g
            select new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) };

        return await query.ToDictionaryAsync(x => x.ProductId, x => x.Total);
    }
}