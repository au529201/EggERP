using EggERP.Application.Suppliers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Suppliers;
public class SupplierRepository : ISupplierRepository
{
    private readonly EggERPDbContext _dbContext;
    public SupplierRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<Supplier>> GetActiveByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Suppliers
            .Where(s => s.BusinessId == businessId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    public Task<Supplier?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.Id == id);
    }
    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();
        return supplier;
    }
    public async Task<bool> UpdateAsync(Supplier supplier)
    {
        var existing = await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.BusinessId == supplier.BusinessId && s.Id == supplier.Id);
        if (existing is null)
        {
            return false;
        }
        existing.Name = supplier.Name;
        existing.ContactPerson = supplier.ContactPerson;
        existing.Email = supplier.Email;
        existing.Phone = supplier.Phone;
        existing.Address = supplier.Address;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeactivateAsync(Guid businessId, Guid id)
    {
        var existing = await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.Id == id);
        if (existing is null)
        {
            return false;
        }
        existing.IsActive = false;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}