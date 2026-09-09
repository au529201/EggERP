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
        supplier.Id = Guid.NewGuid();
        supplier.CreatedAtUtc = DateTime.UtcNow;
        supplier.IsActive = true;
        return await _supplierRepository.AddAsync(supplier);
    }
    public Task<bool> UpdateSupplierAsync(Supplier supplier)
    {
        return _supplierRepository.UpdateAsync(supplier);
    }
    public Task<bool> DeactivateSupplierAsync(Guid businessId, Guid id)
    {
        return _supplierRepository.DeactivateAsync(businessId, id);
    }
}