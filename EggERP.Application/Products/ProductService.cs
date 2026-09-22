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

    public Task<Product?> GetProductByIdAsync(Guid businessId, Guid id)
    {
        return _productRepository.GetByIdAsync(businessId, id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAtUtc = DateTime.UtcNow;
        product.IsActive = true;
        product.Name = ComposeName(product.Type, product.Description);
        return await _productRepository.AddAsync(product);
    }

    public Task<bool> UpdateProductAsync(Product product)
    {
        product.Name = ComposeName(product.Type, product.Description);
        return _productRepository.UpdateAsync(product);
    }

    public Task<bool> DeactivateProductAsync(Guid businessId, Guid id)
    {
        return _productRepository.DeactivateAsync(businessId, id);
    }

    private static string ComposeName(string type, string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? type : $"{type} - {description}";
    }
}