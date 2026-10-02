using EggERP.Domain.Entities;

namespace EggERP.Application.Businesses;

public interface IBusinessRepository
{
    Task<Business?> GetByIdAsync(Guid id);

    Task AddAsync(Business business);

    Task<bool> UpdateAsync(Business business);
}