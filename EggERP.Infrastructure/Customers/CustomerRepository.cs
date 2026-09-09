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
            .Where(c => c.BusinessId == businessId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
    public Task<Customer?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Customers
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);
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
            .FirstOrDefaultAsync(c => c.BusinessId == customer.BusinessId && c.Id == customer.Id);
        if (existing is null)
        {
            return false;
        }
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
        var existing = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);
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