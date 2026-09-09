using EggERP.Application.Purchases;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Purchases;
public class PurchaseRepository : IPurchaseRepository
{
    private readonly EggERPDbContext _dbContext;
    public PurchaseRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<Purchase>> GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Purchases
            .Where(p => p.BusinessId == businessId)
            .OrderByDescending(p => p.PurchaseDateUtc)
            .ToListAsync();
    }
    public Task<Purchase?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == id);
    }
    public Task<List<PurchaseItem>> GetItemsByPurchaseIdAsync(Guid purchaseId)
    {
        return _dbContext.PurchaseItems
            .Where(pi => pi.PurchaseId == purchaseId)
            .ToListAsync();
    }
    public async Task<Purchase> CreateAsync(Purchase purchase, List<PurchaseItem> items)
    {
        _dbContext.Purchases.Add(purchase);
        _dbContext.PurchaseItems.AddRange(items);
        await _dbContext.SaveChangesAsync();
        return purchase;
    }
}