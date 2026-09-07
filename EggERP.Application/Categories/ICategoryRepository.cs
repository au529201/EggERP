using EggERP.Domain.Entities;
namespace EggERP.Application.Categories;
public interface ICategoryRepository
{
    Task<List<Category>> GetActiveByBusinessIdAsync(Guid businessId);
    Task<Category?> GetByIdAsync(Guid businessId, Guid id);
    Task<Category> AddAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeactivateAsync(Guid businessId, Guid id);
}