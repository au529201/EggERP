
using EggERP.Application.ActivityLogs;
using EggERP.Application.Categories;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = "ActiveBusiness")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public CategoryController(
        ICategoryService categoryService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _categoryService = categoryService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetCategories(Guid businessId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (businessId != user.BusinessId)
            return Forbid();

        var categories = await _categoryService
            .GetCategoriesAsync(user.BusinessId);

        return Ok(categories);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(
        Guid businessId,
        Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (businessId != user.BusinessId)
            return Forbid();

        var category = await _categoryService
            .GetCategoryByIdAsync(user.BusinessId, id);

        return category is null
            ? NotFound()
            : Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] Category category)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (category is null)
            return BadRequest("Category details are required.");

        category.BusinessId = user.BusinessId;

        var created = await _categoryService
            .CreateCategoryAsync(category);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Category",
            "Category",
            created.Id,
            created.Name);

        return CreatedAtAction(
            nameof(GetCategoryById),
            new
            {
                businessId = user.BusinessId,
                id = created.Id
            },
            created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] Category category)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (category is null)
            return BadRequest("Category details are required.");

        if (id != category.Id)
        {
            return BadRequest(
                "Route id does not match category id in the request body.");
        }

        var existing = await _categoryService
            .GetCategoryByIdAsync(user.BusinessId, id);

        if (existing is null)
            return NotFound();

        category.BusinessId = user.BusinessId;

        var updated = await _categoryService
            .UpdateCategoryAsync(category);

        if (!updated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Updated Category",
            "Category",
            id,
            category.Name);

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateCategory(
        Guid businessId,
        Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (businessId != user.BusinessId)
            return Forbid();

        var deactivated = await _categoryService
            .DeactivateCategoryAsync(user.BusinessId, id);

        if (!deactivated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Deactivated Category",
            "Category",
            id,
            null);

        return NoContent();
    }
}
