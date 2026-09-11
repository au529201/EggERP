using EggERP.Application.Businesses;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Businesses;
public class BusinessRepository : IBusinessRepository
{
    private readonly EggERPDbContext _dbContext;
    public BusinessRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<Business?> GetByIdAsync(Guid id)
    {
        return _dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == id);
    }
    public async Task<bool> UpdateAsync(Business business)
    {
        var existing = await _dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == business.Id);
        if (existing is null)
        {
            return false;
        }
        existing.Name = business.Name;
        existing.LegalName = business.LegalName;
        existing.TaxIdentificationNumber = business.TaxIdentificationNumber;
        existing.Email = business.Email;
        existing.Phone = business.Phone;
        existing.AddressLine1 = business.AddressLine1;
        existing.AddressLine2 = business.AddressLine2;
        existing.City = business.City;
        existing.Province = business.Province;
        existing.PostalCode = business.PostalCode;
        existing.CountryCode = business.CountryCode;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}