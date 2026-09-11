using EggERP.Domain.Entities;
namespace EggERP.Application.Businesses;
public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;
    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }
    public Task<Business?> GetBusinessAsync(Guid id)
    {
        return _businessRepository.GetByIdAsync(id);
    }
    public Task<bool> UpdateBusinessAsync(Business business)
    {
        return _businessRepository.UpdateAsync(business);
    }
}