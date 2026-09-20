using EggERP.Application.ActivityLogs;
using EggERP.Application.Products;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public ProductController(
        IProductService productService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _productService = productService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetProducts(Guid businessId)
    {
        var products = await _productService.GetProductsAsync(businessId);
        return Ok(products);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid businessId, Guid id)
    {
        var product = await _productService.GetProductByIdAsync(businessId, id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var createdProduct = await _productService.CreateProductAsync(product);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                createdProduct.BusinessId, currentUser.Id, currentUser.FullName,
                "Created Product", "Product", createdProduct.Id, createdProduct.Name);
        }

        return CreatedAtAction(
            nameof(GetProducts),
            new { businessId = createdProduct.BusinessId },
            createdProduct);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Route id does not match product id in the request body.");
        }
        var updated = await _productService.UpdateProductAsync(product);
        if (!updated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                product.BusinessId, currentUser.Id, currentUser.FullName,
                "Updated Product", "Product", id, product.Name);
        }

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateProduct(Guid businessId, Guid id)
    {
        var deactivated = await _productService.DeactivateProductAsync(businessId, id);
        if (!deactivated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                businessId, currentUser.Id, currentUser.FullName,
                "Deactivated Product", "Product", id, null);
        }

        return NoContent();
    }
}