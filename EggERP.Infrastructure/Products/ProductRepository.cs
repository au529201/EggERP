using EggERP.Application.Products;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Products;

public class ProductRepository : IProductRepository
{
    private readonly EggERPDbContext _dbContext;

    public ProductRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Product>> GetActiveByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Products
            .Where(p => p.BusinessId == businessId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}