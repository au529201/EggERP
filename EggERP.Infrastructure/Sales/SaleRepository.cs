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
            .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.Id == id);
    }
    public Task<List<SaleItem>> GetItemsBySaleIdAsync(Guid saleId)
    {
        return _dbContext.SaleItems
            .Where(si => si.SaleId == saleId)
            .ToListAsync();
    }
    public async Task<Sale> CreateAsync(Sale sale, List<SaleItem> items)
    {
        _dbContext.Sales.Add(sale);
        _dbContext.SaleItems.AddRange(items);
        await _dbContext.SaveChangesAsync();
        return sale;
    }
}