
using EggERP.Application.Customers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Customers;

public class CustomerRepository : ICustomerRepository
{
    private readonly EggERPDbContext _dbContext;

    public CustomerRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Customer>> GetActiveByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public Task<List<Customer>> GetAllByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public Task<Customer?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Customers
            .FirstOrDefaultAsync(c =>
                c.BusinessId == businessId && c.Id == id);
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        var existing = await _dbContext.Customers
            .FirstOrDefaultAsync(c =>
                c.BusinessId == customer.BusinessId &&
                c.Id == customer.Id);

        if (existing is null)
            return false;

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Address = customer.Address;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid businessId, Guid id)
    {
        var affected = await _dbContext.Customers
            .Where(c =>
                c.BusinessId == businessId &&
                c.Id == id &&
                c.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.IsActive, false)
                .SetProperty(c => c.UpdatedAtUtc, DateTime.UtcNow));

        return affected == 1;
    }

    public async Task<bool> ReactivateAsync(Guid businessId, Guid id)
    {
        var affected = await _dbContext.Customers
            .Where(c =>
                c.BusinessId == businessId &&
                c.Id == id &&
                !c.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.IsActive, true)
                .SetProperty(c => c.UpdatedAtUtc, DateTime.UtcNow));

        return affected == 1;
    }
}
