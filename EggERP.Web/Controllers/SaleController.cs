using EggERP.Application.Sales;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/sales")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    public SaleController(ISaleService saleService)
    {
        _saleService = saleService;
    }
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetSales(Guid businessId)
    {
        var sales = await _saleService.GetSalesAsync(businessId);
        return Ok(sales);
    }
    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetSaleById(Guid businessId, Guid id)
    {
        var sale = await _saleService.GetSaleByIdAsync(businessId, id);
        if (sale is null)
        {
            return NotFound();
        }
        var items = await _saleService.GetSaleItemsAsync(id);
        return Ok(new { sale, items });
    }
    public class CreateSaleRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CreateSaleItemRequest> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> CreateSale(CreateSaleRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("A sale must have at least one item.");
        }

        try
        {
            var sale = await _saleService.CreateSaleAsync(request.BusinessId, request.CustomerId, request.Items, request.PaymentMethod, request.PaymentSource, request.ReferenceNumber);
            return CreatedAtAction(nameof(GetSales), new { businessId = sale.BusinessId }, sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}