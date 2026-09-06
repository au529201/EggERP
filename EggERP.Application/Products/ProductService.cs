using EggERP.Domain.Entities;

namespace EggERP.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<List<Product>> GetProductsAsync(Guid businessId)
    {
        return _productRepository.GetActiveByBusinessIdAsync(businessId);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAtUtc = DateTime.UtcNow;
        product.IsActive = true;

        return await _productRepository.AddAsync(product);
    }
}