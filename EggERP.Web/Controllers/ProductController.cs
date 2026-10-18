
using EggERP.Application.ActivityLogs;
using EggERP.Application.Categories;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using EggERP.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = "ActiveBusiness")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public ProductController(
        IProductService productService,
        ICategoryService categoryService,
        IInventoryRepository inventoryRepository,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _inventoryRepository = inventoryRepository;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] bool includeInactive = false)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var canManage = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (includeInactive && !canManage) return Forbid();

        var products = includeInactive
            ? await _productService.GetAllProductsAsync(user.BusinessId)
            : await _productService.GetProductsAsync(user.BusinessId);

        var inventory = await _inventoryRepository
            .GetByBusinessIdAsync(user.BusinessId);

        var inventoryByProduct = inventory.ToDictionary(
            i => i.ProductId,
            i => i);

        var result = products.Select(product =>
        {
            inventoryByProduct.TryGetValue(
                product.Id,
                out var inventoryRecord);

            return new ProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Group = product.Group,
                Type = product.Type,
                Name = product.Name,
                Description = product.Description,
                Unit = product.Unit,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                ReorderLevel = inventoryRecord?.ReorderLevel ?? 0,
                IsActive = product.IsActive
            };
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var product = await _productService
            .GetProductByIdAsync(user.BusinessId, id);

        if (product is null)
            return NotFound();

        var inventory = await _inventoryRepository
            .GetByProductIdAsync(user.BusinessId, id);

        var result = new ProductDetailDto
        {
            Id = product.Id,
            BusinessId = product.BusinessId,
            CategoryId = product.CategoryId,
            Group = product.Group,
            Type = product.Type,
            Name = product.Name,
            Description = product.Description,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            Unit = product.Unit,
            ReorderLevel = inventory?.ReorderLevel ?? 0,
            IsActive = product.IsActive
        };

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] Product product)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (product is null)
            return BadRequest("Product details are required.");

        if (product.CategoryId.HasValue &&
            product.CategoryId.Value != Guid.Empty)
        {
            var category = await _categoryService
                .GetCategoryByIdAsync(
                    user.BusinessId,
                    product.CategoryId.Value);

            if (category is null)
            {
                return BadRequest(
                    "The selected category does not belong to your business.");
            }
        }

        // Never trust the BusinessId provided by the client.
        product.BusinessId = user.BusinessId;

        var created = await _productService
            .CreateProductAsync(product);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Product",
            "Product",
            created.Id,
            created.Name);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (request is null)
            return BadRequest("Product details are required.");

        if (id != request.Id)
        {
            return BadRequest(
                "Route id does not match product id in the request body.");
        }

        if (request.ReorderLevel < 0)
        {
            return BadRequest(
                "Low stock warning cannot be negative.");
        }

        var existing = await _productService
            .GetProductByIdAsync(user.BusinessId, id);

        if (existing is null)
            return NotFound();

        if (request.CategoryId.HasValue &&
            request.CategoryId.Value != Guid.Empty)
        {
            var category = await _categoryService
                .GetCategoryByIdAsync(
                    user.BusinessId,
                    request.CategoryId.Value);

            if (category is null)
            {
                return BadRequest(
                    "The selected category does not belong to your business.");
            }
        }

        var product = new Product
        {
            Id = request.Id,
            BusinessId = user.BusinessId,
            CategoryId = request.CategoryId,
            Group = request.Group,
            Type = request.Type,
            Description = request.Description,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Unit = request.Unit,
            IsActive = existing.IsActive
        };

        var updated = await _productService
            .UpdateProductAsync(product);

        if (!updated)
            return NotFound();

        await _inventoryRepository.SetReorderLevelAsync(
            user.BusinessId,
            id,
            request.ReorderLevel);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Updated Product",
            "Product",
            id,
            product.Name);

        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ReactivateProduct(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var reactivated = await _productService.ReactivateProductAsync(user.BusinessId, id);
        if (!reactivated) return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId, user.Id, user.FullName,
            "Reactivated Product", "Product", id, null);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var deactivated = await _productService
            .DeactivateProductAsync(user.BusinessId, id);

        if (!deactivated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Deactivated Product",
            "Product",
            id,
            null);

        return NoContent();
    }
}
