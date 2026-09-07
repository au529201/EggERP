using EggERP.Domain.Entities;
namespace EggERP.Application.Categories;
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public Task<List<Category>> GetCategoriesAsync(Guid businessId)
    {
        return _categoryRepository.GetActiveByBusinessIdAsync(businessId);
    }
    public Task<Category?> GetCategoryByIdAsync(Guid businessId, Guid id)
    {
        return _categoryRepository.GetByIdAsync(businessId, id);
    }
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid();
        category.CreatedAtUtc = DateTime.UtcNow;
        category.IsActive = true;
        return await _categoryRepository.AddAsync(category);
    }
    public Task<bool> UpdateCategoryAsync(Category category)
    {
        return _categoryRepository.UpdateAsync(category);
    }
    public Task<bool> DeactivateCategoryAsync(Guid businessId, Guid id)
    {
        return _categoryRepository.DeactivateAsync(businessId, id);
    }
}