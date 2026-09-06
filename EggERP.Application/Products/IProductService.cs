using EggERP.Domain.Entities;

namespace EggERP.Application.Products;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(Guid businessId);

    Task<Product> CreateProductAsync(Product product);
}