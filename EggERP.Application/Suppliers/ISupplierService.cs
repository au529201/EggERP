using EggERP.Domain.Entities;
namespace EggERP.Application.Suppliers;
public interface ISupplierService
{
    Task<List<Supplier>> GetSuppliersAsync(Guid businessId);
    Task<Supplier?> GetSupplierByIdAsync(Guid businessId, Guid id);
    Task<Supplier> CreateSupplierAsync(Supplier supplier);
    Task<bool> UpdateSupplierAsync(Supplier supplier);
    Task<bool> DeactivateSupplierAsync(Guid businessId, Guid id);
}