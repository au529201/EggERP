using EggERP.Application.Products;
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
}