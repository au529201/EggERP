
using EggERP.Application.ActivityLogs;
using EggERP.Application.Sales;
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Persistence;
using EggERP.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Manager,Staff")]
[Authorize(Policy = "ActiveBusiness")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;
    private readonly EggERPDbContext _dbContext;

    public SaleController(
        ISaleService saleService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService,
        EggERPDbContext dbContext)
    {
        _saleService = saleService;
        _userManager = userManager;
        _activityLogService = activityLogService;
        _dbContext = dbContext;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetSales(Guid businessId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        return Ok(await _saleService.GetSalesAsync(businessId));
    }

    [HttpGet("{businessId:guid}/daily-by-group")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetDailySalesByGroup(
        Guid businessId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var today = DateTime.Today;
        var start = today.AddDays(-6);
        var end = today.AddDays(1);

        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var items = await (
            from sale in _dbContext.Sales.AsNoTracking()
            join item in _dbContext.SaleItems.AsNoTracking()
                on sale.Id equals item.SaleId
            join product in _dbContext.Products.AsNoTracking()
                on item.ProductId equals product.Id
            where sale.BusinessId == businessId
                  && product.BusinessId == businessId
                  && sale.SaleDateUtc >= startUtc
                  && sale.SaleDateUtc < endUtc
            select new
            {
                sale.SaleDateUtc,
                product.Group,
                item.TotalAmount
            }
        ).ToListAsync();

        var result = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = start.AddDays(offset);

                var daily = items.Where(x =>
                    x.SaleDateUtc.ToLocalTime().Date == date);

                return new DailySalesByGroupDto
                {
                    Date = date,
                    FlockSales = daily
                        .Where(x => x.Group == "Flock")
                        .Sum(x => x.TotalAmount),
                    EggSales = daily
                        .Where(x => x.Group == "Egg")
                        .Sum(x => x.TotalAmount)
                };
            })
            .ToList();

        return Ok(result);
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
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            request.BusinessId != user.BusinessId)
            return Forbid();

        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("A sale must have at least one item.");

        try
        {
            var sale = await _saleService.CreateSaleAsync(
                user.BusinessId,
                request.CustomerId,
                request.Items,
                request.PaymentMethod,
                request.PaymentSource,
                request.ReferenceNumber,
                request.Status,
                request.SaleDate,
                request.Notes);

            await _activityLogService.LogAsync(
                user.BusinessId,
                user.Id,
                user.FullName,
                "Created Sale",
                "Sale",
                sale.Id,
                $"{request.Items.Count} item(s) — Total: " +
                $"{sale.TotalAmount:N2} ({request.PaymentMethod})");

            return CreatedAtAction(
                nameof(GetSales),
                new { businessId = user.BusinessId },
                sale);
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
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var (succeeded, error) =
            await _saleService.MarkAsPaidAsync(
                businessId,
                id,
                request.PaymentMethod,
                request.PaymentSource,
                request.ReferenceNumber);

        if (!succeeded)
            return BadRequest(error);

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Sale Payment Completed",
            "Sale",
            id,
            $"Marked as Paid ({request.PaymentMethod})");

        return NoContent();
    }

    [HttpPut("{businessId:guid}/{id:guid}/mark-pending")]
    public async Task<IActionResult> MarkAsPending(
        Guid businessId,
        Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Unauthorized();

        if (user.BusinessId == Guid.Empty ||
            user.BusinessId != businessId)
            return Forbid();

        var (succeeded, error) =
            await _saleService.MarkAsPendingAsync(
                businessId,
                id);

        if (!succeeded)
            return BadRequest(error);

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Sale Marked Pending",
            "Sale",
            id,
            "Payment status changed from Paid to Pending.");

        return NoContent();
    }
}
