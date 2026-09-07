using EggERP.Application.Categories;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetCategories(Guid businessId)
    {
        var categories = await _categoryService.GetCategoriesAsync(businessId);
        return Ok(categories);
    }
    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid businessId, Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(businessId, id);
        if (category is null)
        {
            return NotFound();
        }
        return Ok(category);
    }
    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        var createdCategory = await _categoryService.CreateCategoryAsync(category);
        return CreatedAtAction(
            nameof(GetCategories),
            new { businessId = createdCategory.BusinessId },
            createdCategory);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest("Route id does not match category id in the request body.");
        }
        var updated = await _categoryService.UpdateCategoryAsync(category);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }
    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateCategory(Guid businessId, Guid id)
    {
        var deactivated = await _categoryService.DeactivateCategoryAsync(businessId, id);
        if (!deactivated)
        {
            return NotFound();
        }
        return NoContent();
    }
}