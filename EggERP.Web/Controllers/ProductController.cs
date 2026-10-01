using EggERP.Application.ActivityLogs;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using EggERP.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public ProductController(
        IProductService productService,
        IInventoryRepository inventoryRepository,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _productService = productService;
        _inventoryRepository = inventoryRepository;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var products =
            await _productService.GetProductsAsync(
                currentUser.BusinessId);

        var inventory =
            await _inventoryRepository.GetByBusinessIdAsync(
                currentUser.BusinessId);

        var inventoryByProduct =
            inventory.ToDictionary(
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
                ReorderLevel =
                    inventoryRecord?.ReorderLevel ?? 0,
                IsActive = product.IsActive
            };
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var product =
            await _productService.GetProductByIdAsync(
                currentUser.BusinessId,
                id);

        if (product is null)
        {
            return NotFound();
        }

        var inventory =
            await _inventoryRepository.GetByProductIdAsync(
                currentUser.BusinessId,
                id);

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
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        // Never trust BusinessId sent by the client.
        product.BusinessId = currentUser.BusinessId;

        var createdProduct =
            await _productService.CreateProductAsync(product);

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Created Product",
            "Product",
            createdProduct.Id,
            createdProduct.Name);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = createdProduct.Id },
            createdProduct);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request)
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

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

        var existingProduct =
            await _productService.GetProductByIdAsync(
                currentUser.BusinessId,
                id);

        if (existingProduct is null)
        {
            return NotFound();
        }

        var product = new Product
        {
            Id = request.Id,
            BusinessId = currentUser.BusinessId,
            CategoryId = request.CategoryId,
            Group = request.Group,
            Type = request.Type,
            Description = request.Description,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Unit = request.Unit,
            IsActive = existingProduct.IsActive
        };

        var updated =
            await _productService.UpdateProductAsync(product);

        if (!updated)
        {
            return NotFound();
        }

        await _inventoryRepository.SetReorderLevelAsync(
            currentUser.BusinessId,
            id,
            request.ReorderLevel);

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Updated Product",
            "Product",
            id,
            product.Name);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var deactivated =
            await _productService.DeactivateProductAsync(
                currentUser.BusinessId,
                id);

        if (!deactivated)
        {
            return NotFound();
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Deactivated Product",
            "Product",
            id,
            null);

        return NoContent();
    }
}