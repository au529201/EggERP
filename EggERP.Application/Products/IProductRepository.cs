using EggERP.Domain.Entities;

namespace EggERP.Application.Products;

public interface IProductRepository
{
    Task<List<Product>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Product?> GetByIdAsync(Guid businessId, Guid id);
    Task<Product> AddAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeactivateAsync(Guid businessId, Guid id);
}