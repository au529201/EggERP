using EggERP.Application.Sales;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Sales;

public class SaleRepository : ISaleRepository
{
    private readonly EggERPDbContext _dbContext;

    public SaleRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Sale>> GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Sales
            .Where(s => s.BusinessId == businessId)
            .OrderByDescending(s => s.SaleDateUtc)
            .ToListAsync();
    }

    public Task<Sale?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Sales
            .FirstOrDefaultAsync(
                s => s.BusinessId == businessId &&
                     s.Id == id);
    }

    public Task<List<SaleItem>> GetItemsBySaleIdAsync(
        Guid businessId,
        Guid saleId)
    {
        return _dbContext.SaleItems
            .Where(si =>
                si.SaleId == saleId &&
                _dbContext.Sales.Any(s =>
                    s.Id == si.SaleId &&
                    s.BusinessId == businessId))
            .ToListAsync();
    }

    public async Task<Sale> CreateAsync(
        Sale sale,
        List<SaleItem> items)
    {
        _dbContext.Sales.Add(sale);
        _dbContext.SaleItems.AddRange(items);

        await _dbContext.SaveChangesAsync();

        return sale;
    }


    public async Task<bool> TryMarkAsPaidAsync(
        Guid businessId,
        Guid saleId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber)
    {
        if (businessId == Guid.Empty || saleId == Guid.Empty)
            return false;

        var affectedRows = await _dbContext.Sales
            .Where(s =>
                s.BusinessId == businessId &&
                s.Id == saleId &&
                s.Status == "Pending")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, "Paid")
                .SetProperty(s => s.PaymentMethod, paymentMethod)
                .SetProperty(s => s.PaymentSource, paymentSource)
                .SetProperty(s => s.ReferenceNumber, referenceNumber)
                .SetProperty(s => s.UpdatedAtUtc, DateTime.UtcNow));

        return affectedRows == 1;
    }

}