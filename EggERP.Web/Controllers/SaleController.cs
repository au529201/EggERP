using EggERP.Application.ActivityLogs;
using EggERP.Application.Sales;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public SaleController(
        ISaleService saleService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _saleService = saleService;
        _userManager = userManager;
        _activityLogService = activityLogService;
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
        public string Status { get; set; } = "Paid";
        public DateTime? SaleDate { get; set; }
        public string? Notes { get; set; }
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
            var sale = await _saleService.CreateSaleAsync(
                request.BusinessId,
                request.CustomerId,
                request.Items,
                request.PaymentMethod,
                request.PaymentSource,
                request.ReferenceNumber,
                request.Status,
                request.SaleDate,
                request.Notes);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is not null)
            {
                await _activityLogService.LogAsync(
                    request.BusinessId, currentUser.Id, currentUser.FullName,
                    "Created Sale", "Sale", sale.Id,
                    $"{request.Items.Count} item(s) — Total: {sale.TotalAmount:N2} ({request.PaymentMethod})");
            }

            return CreatedAtAction(nameof(GetSales), new { businessId = sale.BusinessId }, sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public class MarkSalePaidRequest
    {
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    [HttpPut("{businessId:guid}/{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(
        Guid businessId,
        Guid id,
        MarkSalePaidRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (currentUser.BusinessId != businessId)
        {
            return Forbid();
        }

        var (succeeded, error) =
            await _saleService.MarkAsPaidAsync(
                businessId,
                id,
                request.PaymentMethod,
                request.PaymentSource,
                request.ReferenceNumber);

        if (!succeeded)
        {
            return BadRequest(error);
        }

        await _activityLogService.LogAsync(
            currentUser.BusinessId,
            currentUser.Id,
            currentUser.FullName,
            "Sale Payment Completed",
            "Sale",
            id,
            $"Marked as Paid ({request.PaymentMethod})");

        return NoContent();
    }
}