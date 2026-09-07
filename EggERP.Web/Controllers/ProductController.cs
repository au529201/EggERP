using EggERP.Application.Products;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
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

        return NoContent();
    }
}