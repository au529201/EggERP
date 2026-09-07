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

    public Task<Product?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Products
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == id);
    }

    public async Task<Product> AddAsync(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var existing = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.BusinessId == product.BusinessId && p.Id == product.Id);

        if (existing is null)
        {
            return false;
        }

        existing.CategoryId = product.CategoryId;
        existing.Name = product.Name;
        existing.SKU = product.SKU;
        existing.Description = product.Description;
        existing.CostPrice = product.CostPrice;
        existing.SellingPrice = product.SellingPrice;
        existing.Unit = product.Unit;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}