using EggERP.Domain.Entities;
namespace EggERP.Application.Customers;
public interface ICustomerRepository
{
    Task<List<Customer>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Customer?> GetByIdAsync(Guid businessId, Guid id);
    Task<Customer> AddAsync(Customer customer);
    Task<bool> UpdateAsync(Customer customer);
    Task<bool> DeactivateAsync(Guid businessId, Guid id);
}