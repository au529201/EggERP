using EggERP.Domain.Entities;
namespace EggERP.Application.Businesses;
public interface IBusinessService
{
    Task<Business?> GetBusinessAsync(Guid id);
    Task<bool> UpdateBusinessAsync(Business business);
}