using Microsoft.Data.SqlClient;
using EggERP.Application.Inventory;
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
        GetByProductIdAsync(Guid businessId, Guid productId)
    {
        return _dbContext.Inventories
            .FirstOrDefaultAsync(i =>
                i.BusinessId == businessId &&
                i.ProductId == productId);
    }

    private async Task ValidateProductAsync(
        Guid businessId,
        Guid productId)
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
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        AdjustQuantityAsync(
            Guid businessId,
            Guid productId,
            decimal delta)
    {
        await ValidateProductAsync(businessId, productId);

        if (delta == 0)
        {
            throw new InvalidOperationException(
                "Stock adjustment must not be zero.");
        }

        // Deduct stock atomically.
        // SQL checks the available quantity and updates it
        // in the same database operation.
        if (delta < 0)
        {
            var affectedRows = await _dbContext.Inventories
                .Where(i =>
                    i.BusinessId == businessId &&
                    i.ProductId == productId &&
                    i.QuantityOnHand >= -delta)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        i => i.QuantityOnHand,
                        i => i.QuantityOnHand + delta)
                    .SetProperty(
                        i => i.UpdatedAtUtc,
                        DateTime.UtcNow));

            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Insufficient stock. Another transaction may " +
                    "have changed the available quantity. " +
                    "Refresh the inventory and try again.");
            }

            // ExecuteUpdateAsync bypasses EF Core change tracking.
            // Refresh a previously tracked entity to avoid
            // returning stale inventory values.
            var trackedInventory = _dbContext.Inventories.Local
                .FirstOrDefault(i =>
                    i.BusinessId == businessId &&
                    i.ProductId == productId);

            if (trackedInventory is not null)
            {
                await _dbContext.Entry(trackedInventory)
                    .ReloadAsync();

                return trackedInventory;
            }

            return await GetByProductIdAsync(
                businessId,
                productId)
                ?? throw new InvalidOperationException(
                    "Inventory record not found after adjustment.");
        }

        // Add stock.
        // For an existing row, use an atomic SQL update
        // so concurrent additions do not overwrite each other.
        var addedRows = await _dbContext.Inventories
            .Where(i =>
                i.BusinessId == businessId &&
                i.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    i => i.QuantityOnHand,
                    i => i.QuantityOnHand + delta)
                .SetProperty(
                    i => i.UpdatedAtUtc,
                    DateTime.UtcNow));

        if (addedRows > 0)
        {
            var trackedAddedInventory = _dbContext.Inventories.Local
                .FirstOrDefault(i =>
                    i.BusinessId == businessId &&
                    i.ProductId == productId);

            if (trackedAddedInventory is not null)
            {
                await _dbContext.Entry(trackedAddedInventory)
                    .ReloadAsync();

                return trackedAddedInventory;
            }

            return await GetByProductIdAsync(
                businessId,
                productId)
                ?? throw new InvalidOperationException(
                    "Inventory record not found after adding stock.");
        }

        // No inventory record exists yet.
        // The unique database index being added separately
        // will prevent duplicate business/product rows.
        var newInventory = new EggERP.Domain.Entities.Inventory
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ProductId = productId,
            QuantityOnHand = delta,
            ReorderLevel = 0,
            HighStockThreshold = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Inventories.Add(newInventory);

        try
        {
            await _dbContext.SaveChangesAsync();
            return newInventory;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // Another request inserted the inventory row first.
            // Remove our failed insert from EF change tracking.
            _dbContext.Entry(newInventory).State = EntityState.Detached;

            // Atomically add our quantity to the existing row.
            var updatedRows = await _dbContext.Inventories
                .Where(i =>
                    i.BusinessId == businessId &&
                    i.ProductId == productId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        i => i.QuantityOnHand,
                        i => i.QuantityOnHand + delta)
                    .SetProperty(
                        i => i.UpdatedAtUtc,
                        DateTime.UtcNow));

            if (updatedRows != 1)
            {
                throw new InvalidOperationException(
                    "Inventory changed during the transaction. Please retry.");
            }

            return await GetByProductIdAsync(businessId, productId)
                ?? throw new InvalidOperationException(
                    "Inventory record not found after concurrent addition.");
        }
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        SetReorderLevelAsync(
            Guid businessId,
            Guid productId,
            decimal reorderLevel)
    {
        await ValidateProductAsync(businessId, productId);

        var existing = await GetByProductIdAsync(
            businessId,
            productId);

        return await SetThresholdsAsync(
            businessId,
            productId,
            reorderLevel,
            existing?.HighStockThreshold);
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        SetThresholdsAsync(
            Guid businessId,
            Guid productId,
            decimal lowThreshold,
            decimal? highThreshold)
    {
        await ValidateProductAsync(businessId, productId);

        if (lowThreshold < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lowThreshold),
                "Low threshold cannot be negative.");
        }

        if (highThreshold.HasValue &&
            highThreshold.Value <= lowThreshold)
        {
            throw new ArgumentException(
                "High threshold must be greater than low threshold.");
        }

        var existing = await GetByProductIdAsync(
            businessId,
            productId);

        if (existing is null)
        {
            existing = new EggERP.Domain.Entities.Inventory
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                ProductId = productId,
                QuantityOnHand = 0,
                ReorderLevel = lowThreshold,
                HighStockThreshold = highThreshold,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Inventories.Add(existing);
        }
        else
        {
            existing.ReorderLevel = lowThreshold;
            existing.HighStockThreshold = highThreshold;
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
            from purchaseItem in _dbContext.PurchaseItems
            join purchase in _dbContext.Purchases
                on purchaseItem.PurchaseId equals purchase.Id
            where purchase.BusinessId == businessId &&
                  purchase.PurchaseDateUtc < endOfDay
            group purchaseItem by purchaseItem.ProductId into grouped
            select new
            {
                ProductId = grouped.Key,
                Total = grouped.Sum(x => x.Quantity)
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
            from saleItem in _dbContext.SaleItems
            join sale in _dbContext.Sales
                on saleItem.SaleId equals sale.Id
            where sale.BusinessId == businessId &&
                  sale.SaleDateUtc < endOfDay
            group saleItem by saleItem.ProductId into grouped
            select new
            {
                ProductId = grouped.Key,
                Total = grouped.Sum(x => x.Quantity)
            };

        return await query.ToDictionaryAsync(
            x => x.ProductId,
            x => x.Total);
    }
}
