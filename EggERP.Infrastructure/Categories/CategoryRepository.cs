using EggERP.Application.Categories;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Categories;
public class CategoryRepository : ICategoryRepository
{
    private readonly EggERPDbContext _dbContext;
    public CategoryRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<Category>> GetActiveByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Categories
            .Where(c => c.BusinessId == businessId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
    public Task<Category?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);
    }
    public async Task<Category> AddAsync(Category category)
    {
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }
    public async Task<bool> UpdateAsync(Category category)
    {
        var existing = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == category.BusinessId && c.Id == category.Id);
        if (existing is null)
        {
            return false;
        }
        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeactivateAsync(Guid businessId, Guid id)
    {
        var existing = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);
        if (existing is null)
        {
            return false;
        }
        existing.IsActive = false;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}