using EggERP.Domain.Entities;

namespace EggERP.Application.Products;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(Guid businessId);
    Task<List<Product>> GetAllProductsAsync(Guid businessId);
    Task<Product?> GetProductByIdAsync(Guid businessId, Guid id);
    Task<Product> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeactivateProductAsync(Guid businessId, Guid id);
    Task<bool> ReactivateProductAsync(Guid businessId, Guid id);
}