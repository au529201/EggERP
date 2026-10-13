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

    public Task<List<EggERP.Domain.Entities.Inventory>>
        GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Inventories
            .Where(i => i.BusinessId == businessId)
            .ToListAsync();
    }

    public Task<EggERP.Domain.Entities.Inventory?>
        GetByProductIdAsync(
            Guid businessId,
            Guid productId)
    {
        return _dbContext.Inventories
            .FirstOrDefaultAsync(i =>
                i.BusinessId == businessId &&
                i.ProductId == productId);
    }
    public async Task<EggERP.Domain.Entities.Inventory>
    AdjustQuantityAsync(
        Guid businessId,
        Guid productId,
        decimal delta)
    {
        if (businessId == Guid.Empty || productId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid business and product are required.");
        }

        var productExists = await _dbContext.Products
            .AnyAsync(p =>
                p.Id == productId &&
                p.BusinessId == businessId);

        if (!productExists)
        {
            throw new InvalidOperationException(
                "Product not found in this business.");
        }

        var existing = await _dbContext.Inventories
            .FirstOrDefaultAsync(i =>
                i.BusinessId == businessId &&
                i.ProductId == productId);

        if (existing is null)
        {
            if (delta < 0)
            {
                throw new InvalidOperationException(
                    "Cannot remove stock because no inventory record exists.");
            }

            var newRecord = new EggERP.Domain.Entities.Inventory
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                ProductId = productId,
                QuantityOnHand = delta,
                ReorderLevel = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Inventories.Add(newRecord);
            await _dbContext.SaveChangesAsync();

            return newRecord;
        }

        var newQuantity = existing.QuantityOnHand + delta;

        if (newQuantity < 0)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {existing.QuantityOnHand:N2}, " +
                $"requested removal: {Math.Abs(delta):N2}.");
        }

        existing.QuantityOnHand = newQuantity;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return existing;
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        SetReorderLevelAsync(
            Guid businessId,
            Guid productId,
            decimal reorderLevel)
    {
        if (reorderLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reorderLevel),
                "Reorder level cannot be negative.");
        }

        var existing = await _dbContext.Inventories
            .FirstOrDefaultAsync(i =>
                i.BusinessId == businessId &&
                i.ProductId == productId);

        if (existing is null)
        {
            existing =
                new EggERP.Domain.Entities.Inventory
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    ProductId = productId,
                    QuantityOnHand = 0,
                    ReorderLevel = reorderLevel,
                    CreatedAtUtc = DateTime.UtcNow
                };

            _dbContext.Inventories.Add(existing);
        }
        else
        {
            existing.ReorderLevel = reorderLevel;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return existing;
    }

    public async Task<Dictionary<Guid, decimal>>
        GetBoughtQuantitiesAsOfDateAsync(
            Guid businessId,
            DateTime asOfDate)
    {
        var endOfDay = asOfDate.Date.AddDays(1);

        var query =
            from pi in _dbContext.PurchaseItems
            join p in _dbContext.Purchases
                on pi.PurchaseId equals p.Id
            where
                p.BusinessId == businessId &&
                p.PurchaseDateUtc < endOfDay
            group pi by pi.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                Total = g.Sum(x => x.Quantity)
            };

        return await query.ToDictionaryAsync(
            x => x.ProductId,
            x => x.Total);
    }

    public async Task<Dictionary<Guid, decimal>>
        GetSoldQuantitiesAsOfDateAsync(
            Guid businessId,
            DateTime asOfDate)
    {
        var endOfDay = asOfDate.Date.AddDays(1);

        var query =
            from si in _dbContext.SaleItems
            join s in _dbContext.Sales
                on si.SaleId equals s.Id
            where
                s.BusinessId == businessId &&
                s.SaleDateUtc < endOfDay
            group si by si.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                Total = g.Sum(x => x.Quantity)
            };

        return await query.ToDictionaryAsync(
            x => x.ProductId,
            x => x.Total);
    }
}