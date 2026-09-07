using EggERP.Domain.Entities;
namespace EggERP.Application.Categories;
public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync(Guid businessId);
    Task<Category?> GetCategoryByIdAsync(Guid businessId, Guid id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Category category);
    Task<bool> DeactivateCategoryAsync(Guid businessId, Guid id);
}