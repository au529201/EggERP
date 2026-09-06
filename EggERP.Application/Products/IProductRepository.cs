using EggERP.Domain.Entities;

namespace EggERP.Application.Products;

public interface IProductRepository
{
    Task<List<Product>> GetActiveByBusinessIdAsync(Guid businessId);
}