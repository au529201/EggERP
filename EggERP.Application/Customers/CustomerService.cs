using EggERP.Domain.Entities;
namespace EggERP.Application.Customers;
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public Task<List<Customer>> GetCustomersAsync(Guid businessId)
    {
        return _customerRepository.GetActiveByBusinessIdAsync(businessId);
    }
    public Task<Customer?> GetCustomerByIdAsync(Guid businessId, Guid id)
    {
        return _customerRepository.GetByIdAsync(businessId, id);
    }
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.Id = Guid.NewGuid();
        customer.CreatedAtUtc = DateTime.UtcNow;
        customer.IsActive = true;
        return await _customerRepository.AddAsync(customer);
    }
    public Task<bool> UpdateCustomerAsync(Customer customer)
    {
        return _customerRepository.UpdateAsync(customer);
    }
    public Task<bool> DeactivateCustomerAsync(Guid businessId, Guid id)
    {
        return _customerRepository.DeactivateAsync(businessId, id);
    }
}