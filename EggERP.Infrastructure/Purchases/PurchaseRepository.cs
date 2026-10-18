
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
            .FirstOrDefaultAsync(p =>
                p.BusinessId == businessId &&
                p.Id == id);
    }

    public Task<List<PurchaseItem>> GetItemsByPurchaseIdAsync(
        Guid businessId,
        Guid purchaseId)
    {
        return _dbContext.PurchaseItems
            .Where(pi =>
                pi.PurchaseId == purchaseId &&
                _dbContext.Purchases.Any(p =>
                    p.Id == pi.PurchaseId &&
                    p.BusinessId == businessId))
            .ToListAsync();
    }

    public async Task<Purchase> CreateAsync(
        Purchase purchase,
        List<PurchaseItem> items)
    {
        _dbContext.Purchases.Add(purchase);
        _dbContext.PurchaseItems.AddRange(items);

        await _dbContext.SaveChangesAsync();

        return purchase;
    }

    public async Task<bool> TryMarkAsPaidAsync(
        Guid businessId,
        Guid purchaseId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string? bankName)
    {
        if (businessId == Guid.Empty || purchaseId == Guid.Empty)
            return false;

        var updatedAtUtc = DateTime.UtcNow;

        var affectedRows = await _dbContext.Purchases
            .Where(p =>
                p.BusinessId == businessId &&
                p.Id == purchaseId &&
                p.Status == "Pending")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Status, "Paid")
                .SetProperty(p => p.PaymentMethod, paymentMethod)
                .SetProperty(p => p.PaymentSource, paymentSource)
                .SetProperty(p => p.ReferenceNumber, referenceNumber)
                .SetProperty(p => p.BankName, bankName)
                .SetProperty(p => p.UpdatedAtUtc, updatedAtUtc));

        return affectedRows == 1;
    }

    public async Task<bool> TryMarkAsPendingAsync(
        Guid businessId,
        Guid purchaseId)
    {
        if (businessId == Guid.Empty || purchaseId == Guid.Empty)
            return false;

        var updatedAtUtc = DateTime.UtcNow;

        var affectedRows = await _dbContext.Purchases
            .Where(p =>
                p.BusinessId == businessId &&
                p.Id == purchaseId &&
                p.Status == "Paid")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Status, "Pending")
                .SetProperty(p => p.UpdatedAtUtc, updatedAtUtc));

        return affectedRows == 1;
    }
}
