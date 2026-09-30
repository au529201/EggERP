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

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var products = await _productService.GetProductsAsync(currentUser.BusinessId);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var product = await _productService.GetProductByIdAsync(
            currentUser.BusinessId,
            id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        // Never trust BusinessId sent by the client.
        product.BusinessId = currentUser.BusinessId;

        var createdProduct = await _productService.CreateProductAsync(product);

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
    public async Task<IActionResult> UpdateProduct(Guid id, Product product)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (id != product.Id)
        {
            return BadRequest(
                "Route id does not match product id in the request body.");
        }

        // Never trust BusinessId sent by the client.
        product.BusinessId = currentUser.BusinessId;

        var updated = await _productService.UpdateProductAsync(product);

        if (!updated)
        {
            return NotFound();
        }

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
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var deactivated = await _productService.DeactivateProductAsync(
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