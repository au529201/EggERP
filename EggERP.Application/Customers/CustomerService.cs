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
        ValidateCustomer(customer);

        customer.Id = Guid.NewGuid();
        customer.CreatedAtUtc = DateTime.UtcNow;
        customer.IsActive = true;

        NormalizeCustomer(customer);

        return await _customerRepository.AddAsync(customer);
    }

    public Task<bool> UpdateCustomerAsync(Customer customer)
    {
        ValidateCustomer(customer);
        NormalizeCustomer(customer);

        return _customerRepository.UpdateAsync(customer);
    }

    public Task<bool> DeactivateCustomerAsync(Guid businessId, Guid id)
    {
        return _customerRepository.DeactivateAsync(businessId, id);
    }

    private static void ValidateCustomer(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            throw new InvalidOperationException(
                "Customer name is required.");
        }
    }

    private static void NormalizeCustomer(Customer customer)
    {
        customer.Name = customer.Name.Trim();

        customer.Email = string.IsNullOrWhiteSpace(customer.Email)
            ? null
            : customer.Email.Trim();

        customer.Phone = string.IsNullOrWhiteSpace(customer.Phone)
            ? null
            : customer.Phone.Trim();

        customer.Address = string.IsNullOrWhiteSpace(customer.Address)
            ? null
            : customer.Address.Trim();
    }
}