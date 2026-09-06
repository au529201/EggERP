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
}