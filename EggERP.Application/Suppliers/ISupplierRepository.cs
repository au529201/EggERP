using EggERP.Domain.Entities;
namespace EggERP.Application.Suppliers;
public interface ISupplierRepository
{
    Task<List<Supplier>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Supplier?> GetByIdAsync(Guid businessId, Guid id);
    Task<Supplier> AddAsync(Supplier supplier);
    Task<bool> UpdateAsync(Supplier supplier);
    Task<bool> DeactivateAsync(Guid businessId, Guid id);
}