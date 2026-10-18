
using EggERP.Domain.Entities;

namespace EggERP.Application.Customers;

public interface ICustomerService
{
    Task<List<Customer>> GetCustomersAsync(Guid businessId);
    Task<List<Customer>> GetAllCustomersAsync(Guid businessId);
    Task<Customer?> GetCustomerByIdAsync(Guid businessId, Guid id);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeactivateCustomerAsync(Guid businessId, Guid id);
    Task<bool> ReactivateCustomerAsync(Guid businessId, Guid id);
}
