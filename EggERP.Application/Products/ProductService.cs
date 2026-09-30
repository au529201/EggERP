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
        ValidateProduct(product);

        product.Id = Guid.NewGuid();
        product.CreatedAtUtc = DateTime.UtcNow;
        product.IsActive = true;

        product.Type = product.Type.Trim();
        product.Description = string.IsNullOrWhiteSpace(product.Description)
            ? null
            : product.Description.Trim();
        product.Unit = product.Unit.Trim();

        product.Name = ComposeName(product.Type, product.Description);

        return await _productRepository.AddAsync(product);
    }

    public Task<bool> UpdateProductAsync(Product product)
    {
        ValidateProduct(product);

        product.Type = product.Type.Trim();
        product.Description = string.IsNullOrWhiteSpace(product.Description)
            ? null
            : product.Description.Trim();
        product.Unit = product.Unit.Trim();

        product.Name = ComposeName(product.Type, product.Description);

        return _productRepository.UpdateAsync(product);
    }

    public Task<bool> DeactivateProductAsync(Guid businessId, Guid id)
    {
        return _productRepository.DeactivateAsync(businessId, id);
    }
    private static void ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Type))
        {
            throw new InvalidOperationException("Product type is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Unit))
        {
            throw new InvalidOperationException("Product unit is required.");
        }

        if (product.Group != "Flock" && product.Group != "Egg")
        {
            throw new InvalidOperationException(
                "Product group must be either 'Flock' or 'Egg'.");
        }

        if (product.CostPrice < 0)
        {
            throw new InvalidOperationException(
                "Cost price cannot be negative.");
        }

        if (product.SellingPrice < 0)
        {
            throw new InvalidOperationException(
                "Selling price cannot be negative.");
        }
    }
    private static string ComposeName(string type, string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? type : $"{type} - {description}";
    }
}