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
        ValidateCategory(category);

        category.Id = Guid.NewGuid();
        category.CreatedAtUtc = DateTime.UtcNow;
        category.IsActive = true;

        category.Name = category.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(category.Description)
            ? null
            : category.Description.Trim();

        return await _categoryRepository.AddAsync(category);
    }

    public Task<bool> UpdateCategoryAsync(Category category)
    {
        ValidateCategory(category);

        category.Name = category.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(category.Description)
            ? null
            : category.Description.Trim();

        return _categoryRepository.UpdateAsync(category);
    }

    public Task<bool> DeactivateCategoryAsync(Guid businessId, Guid id)
    {
        return _categoryRepository.DeactivateAsync(businessId, id);
    }

    private static void ValidateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new InvalidOperationException(
                "Category name is required.");
        }
    }
}