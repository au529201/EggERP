using EggERP.Application.Flocks;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Flocks;

public class FlockService : IFlockService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly EggERPDbContext _dbContext;

    public FlockService(
        IProductRepository productRepository,
        IInventoryService inventoryService,
        EggERPDbContext dbContext)
    {
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _dbContext = dbContext;
    }

    public async Task<List<StockBoardRowDto>> GetBoardAsync(Guid businessId, string group)
    {
        var products = await _productRepository.GetActiveByBusinessIdAsync(businessId);
        var groupProducts = products.Where(p => p.Group == group).ToList();

        var rows = new List<StockBoardRowDto>();

        foreach (var product in groupProducts)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(businessId, product.Id);
            var available = inventory?.QuantityOnHand ?? 0;

            var bought = await _dbContext.PurchaseItems
                .Where(pi => pi.ProductId == product.Id)
                .SumAsync(pi => (decimal?)pi.Quantity) ?? 0;

            var sold = await _dbContext.SaleItems
                .Where(si => si.ProductId == product.Id)
                .SumAsync(si => (decimal?)si.Quantity) ?? 0;

            var totalSales = await _dbContext.SaleItems
                .Where(si => si.ProductId == product.Id)
                .SumAsync(si => (decimal?)si.TotalAmount) ?? 0;

            rows.Add(new StockBoardRowDto
            {
                ProductId = product.Id,
                Type = product.Type,
                Description = product.Description ?? string.Empty,
                Unit = product.Unit,
                Available = (int)Math.Round(available),
                Bought = (int)Math.Round(bought),
                Sold = (int)Math.Round(sold),
                TotalSales = totalSales
            });
        }

        return rows;
    }

    public async Task<(bool Succeeded, string? Error)> AddStockAsync(AddStockRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.BusinessId, request.ProductId);
        if (product is null)
        {
            return (false, "Product not found in this business.");
        }

        if (product.Group != "Flock" && product.Group != "Egg")
        {
            return (false, "Selected product is not a Flock or Egg product.");
        }

        if (request.Reason == "Bought")
        {
            return (false, "Bought stock is recorded automatically through the New Purchase page.");
        }

        var validReasons = product.Group == "Flock" ? FlockOptions.FlockInReasons : FlockOptions.EggInReasons;
        if (!validReasons.Contains(request.Reason))
        {
            return (false, $"'{request.Reason}' is not a valid reason for adding {product.Group} stock.");
        }

        if (request.Quantity <= 0)
        {
            return (false, "Quantity must be greater than zero.");
        }

        await _inventoryService.AdjustQuantityAsync(request.BusinessId, request.ProductId, request.Quantity);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> RemoveStockAsync(RemoveStockRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.BusinessId, request.ProductId);
        if (product is null)
        {
            return (false, "Product not found in this business.");
        }

        if (product.Group != "Flock" && product.Group != "Egg")
        {
            return (false, "Selected product is not a Flock or Egg product.");
        }

        if (request.Reason == "Sold")
        {
            return (false, "Sold stock is recorded automatically through the New Sale page.");
        }

        var validReasons = product.Group == "Flock" ? FlockOptions.FlockOutReasons : FlockOptions.EggOutReasons;
        if (!validReasons.Contains(request.Reason))
        {
            return (false, $"'{request.Reason}' is not a valid reason for removing {product.Group} stock.");
        }

        if (request.Quantity <= 0)
        {
            return (false, "Quantity must be greater than zero.");
        }

        var inventory = await _inventoryService.GetByProductIdAsync(request.BusinessId, request.ProductId);
        var available = inventory?.QuantityOnHand ?? 0;

        if (request.Quantity > available)
        {
            return (false, $"Quantity exceeds current available stock ({(int)available}).");
        }

        await _inventoryService.AdjustQuantityAsync(request.BusinessId, request.ProductId, -request.Quantity);

        return (true, null);
    }
}