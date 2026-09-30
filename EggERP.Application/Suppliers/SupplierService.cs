using EggERP.Domain.Entities;

namespace EggERP.Application.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public Task<List<Supplier>> GetSuppliersAsync(Guid businessId)
    {
        return _supplierRepository.GetActiveByBusinessIdAsync(businessId);
    }

    public Task<Supplier?> GetSupplierByIdAsync(Guid businessId, Guid id)
    {
        return _supplierRepository.GetByIdAsync(businessId, id);
    }

    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        ValidateSupplier(supplier);

        supplier.Id = Guid.NewGuid();
        supplier.CreatedAtUtc = DateTime.UtcNow;
        supplier.IsActive = true;

        NormalizeSupplier(supplier);

        return await _supplierRepository.AddAsync(supplier);
    }

    public Task<bool> UpdateSupplierAsync(Supplier supplier)
    {
        ValidateSupplier(supplier);
        NormalizeSupplier(supplier);

        return _supplierRepository.UpdateAsync(supplier);
    }

    public Task<bool> DeactivateSupplierAsync(Guid businessId, Guid id)
    {
        return _supplierRepository.DeactivateAsync(businessId, id);
    }

    private static void ValidateSupplier(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
        {
            throw new InvalidOperationException(
                "Supplier name is required.");
        }
    }

    private static void NormalizeSupplier(Supplier supplier)
    {
        supplier.Name = supplier.Name.Trim();

        supplier.ContactPerson =
            string.IsNullOrWhiteSpace(supplier.ContactPerson)
                ? null
                : supplier.ContactPerson.Trim();

        supplier.Email = string.IsNullOrWhiteSpace(supplier.Email)
            ? null
            : supplier.Email.Trim();

        supplier.Phone = string.IsNullOrWhiteSpace(supplier.Phone)
            ? null
            : supplier.Phone.Trim();

        supplier.Address = string.IsNullOrWhiteSpace(supplier.Address)
            ? null
            : supplier.Address.Trim();
    }
}