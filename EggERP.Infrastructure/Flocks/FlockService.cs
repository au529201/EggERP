using EggERP.Application;
using EggERP.Application.Flocks;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Application.Purchases;
using EggERP.Application.Sales;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Flocks;

public class FlockService : IFlockService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IPurchaseService _purchaseService;
    private readonly ISaleService _saleService;
    private readonly EggERPDbContext _dbContext;

    public FlockService(
        IProductRepository productRepository,
        IInventoryService inventoryService,
        IPurchaseService purchaseService,
        ISaleService saleService,
        EggERPDbContext dbContext)
    {
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _purchaseService = purchaseService;
        _saleService = saleService;
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
        if (product is null) return (false, "Product not found in this business.");
        if (product.Group != "Flock" && product.Group != "Egg") return (false, "Selected product is not a Flock or Egg product.");
        if (request.Reason == "Bought") return (false, "Bought stock is recorded automatically through the New Purchase page.");

        var validReasons = product.Group == "Flock" ? FlockOptions.FlockInReasons : FlockOptions.EggInReasons;
        if (!validReasons.Contains(request.Reason)) return (false, $"'{request.Reason}' is not a valid reason for adding {product.Group} stock.");
        if (request.Quantity <= 0) return (false, "Quantity must be greater than zero.");

        await _inventoryService.AdjustQuantityAsync(request.BusinessId, request.ProductId, request.Quantity);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> RemoveStockAsync(RemoveStockRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.BusinessId, request.ProductId);
        if (product is null) return (false, "Product not found in this business.");
        if (product.Group != "Flock" && product.Group != "Egg") return (false, "Selected product is not a Flock or Egg product.");
        if (request.Reason == "Sold") return (false, "Sold stock is recorded automatically through the New Sale page.");

        var validReasons = product.Group == "Flock" ? FlockOptions.FlockOutReasons : FlockOptions.EggOutReasons;
        if (!validReasons.Contains(request.Reason)) return (false, $"'{request.Reason}' is not a valid reason for removing {product.Group} stock.");
        if (request.Quantity <= 0) return (false, "Quantity must be greater than zero.");

        var inventory = await _inventoryService.GetByProductIdAsync(request.BusinessId, request.ProductId);
        var available = inventory?.QuantityOnHand ?? 0;
        if (request.Quantity > available) return (false, $"Quantity exceeds current available stock ({(int)available}).");

        await _inventoryService.AdjustQuantityAsync(request.BusinessId, request.ProductId, -request.Quantity);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ProcessTransactionAsync(
     ProcessStockTransactionRequest request)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            return (false, "At least one line is required.");

        if (request.Direction != "In" && request.Direction != "Out")
            return (false, "Direction must be 'In' or 'Out'.");

        bool IsPaymentLine(StockTransactionLineRequest line) =>
            (request.Direction == "In" && line.Reason == "Bought") ||
            (request.Direction == "Out" && line.Reason == "Sold");

        var hasPaymentLine = request.Lines.Any(IsPaymentLine);

        try
        {
            if (hasPaymentLine)
            {
                if (request.Status != "Paid" && request.Status != "Pending")
                    return (false, "Status must be 'Paid' or 'Pending'.");

                PaymentValidation.EnsureValid(
                    request.PaymentMethod,
                    request.ReferenceNumber,
                    request.PaymentSource);
            }

            // ------------------------------------------------------------
            // 1. VALIDATE EVERY LINE
            // ------------------------------------------------------------
            foreach (var line in request.Lines)
            {
                if (line.Group != "Flock" && line.Group != "Egg")
                    return (false, "Each line must specify Group 'Flock' or 'Egg'.");

                if (line.ProductId == Guid.Empty)
                    return (false, "A product must be selected on every line.");

                if (line.Quantity <= 0)
                    return (false, "Quantity must be greater than zero on every line.");

                var product = await _productRepository.GetByIdAsync(
                    request.BusinessId,
                    line.ProductId);

                if (product is null)
                    return (false, "A selected product was not found in this business.");

                if (product.Group != line.Group)
                    return (false,
                        $"The selected product does not belong to the {line.Group} group.");

                var validReasons = request.Direction == "In"
                    ? (line.Group == "Flock"
                        ? FlockOptions.FlockInReasons
                        : FlockOptions.EggInReasons)
                    : (line.Group == "Flock"
                        ? FlockOptions.FlockOutReasons
                        : FlockOptions.EggOutReasons);

                if (!validReasons.Contains(line.Reason))
                    return (false,
                        $"'{line.Reason}' is not a valid reason for a {line.Group} line.");

                // Bought-specific validation
                if (request.Direction == "In" && line.Reason == "Bought")
                {
                    if (!line.SupplierId.HasValue ||
                        line.SupplierId.Value == Guid.Empty)
                    {
                        return (false,
                            "A supplier is required for every Bought line.");
                    }

                    if (line.UnitCost < 0)
                        return (false, "Unit cost cannot be negative.");
                }

                // Sold-specific validation
                if (request.Direction == "Out" && line.Reason == "Sold")
                {
                    if (!line.CustomerId.HasValue ||
                        line.CustomerId.Value == Guid.Empty)
                    {
                        return (false,
                            "A customer is required for every Sold line.");
                    }

                    if (line.UnitPrice < 0)
                        return (false, "Unit price cannot be negative.");
                }
            }

            // ------------------------------------------------------------
            // 2. VALIDATE TOTAL OUTGOING STOCK PER PRODUCT
            // ------------------------------------------------------------
            // Important:
            // Sold + other Out reasons must be checked together.
            //
            // Example:
            // Available = 100
            // Sold = 60
            // Died = 50
            // Total requested = 110 -> reject entire transaction.
            // ------------------------------------------------------------
            if (request.Direction == "Out")
            {
                var outgoingByProduct = request.Lines
                    .GroupBy(line => line.ProductId)
                    .Select(group => new
                    {
                        ProductId = group.Key,
                        Quantity = group.Sum(line => (decimal)line.Quantity)
                    })
                    .ToList();

                foreach (var outgoing in outgoingByProduct)
                {
                    var inventory = await _inventoryService.GetByProductIdAsync(
                        request.BusinessId,
                        outgoing.ProductId);

                    var available = inventory?.QuantityOnHand ?? 0;

                    if (outgoing.Quantity > available)
                    {
                        return (false,
                            $"Cannot remove {outgoing.Quantity:N0} unit(s). " +
                            $"Only {available:N0} unit(s) are currently available.");
                    }
                }
            }

            // ------------------------------------------------------------
            // 3. BEGIN ONE DATABASE TRANSACTION
            // ------------------------------------------------------------
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // --------------------------------------------------------
                // 4. DIRECT STOCK MOVEMENTS
                // --------------------------------------------------------
                // Bought and Sold are excluded because PurchaseService /
                // SaleService already update Inventory.
                // --------------------------------------------------------
                foreach (var line in request.Lines.Where(l => !IsPaymentLine(l)))
                {
                    var delta = request.Direction == "In"
                        ? (decimal)line.Quantity
                        : -(decimal)line.Quantity;

                    await _inventoryService.AdjustQuantityAsync(
                        request.BusinessId,
                        line.ProductId,
                        delta);
                }

                // --------------------------------------------------------
                // 5. BOUGHT LINES -> REAL PURCHASE RECORDS
                // --------------------------------------------------------
                if (request.Direction == "In")
                {
                    var boughtGroups = request.Lines
                        .Where(line => line.Reason == "Bought")
                        .GroupBy(line => line.SupplierId);

                    foreach (var group in boughtGroups)
                    {
                        var items = group
                            .Select(line => new CreatePurchaseItemRequest
                            {
                                ProductId = line.ProductId,
                                Quantity = line.Quantity,
                                UnitCost = line.UnitCost
                            })
                            .ToList();

                        await _purchaseService.CreatePurchaseAsync(
                            request.BusinessId,
                            group.Key,
                            items,
                            request.PaymentMethod,
                            request.PaymentSource,
                            request.ReferenceNumber,
                            request.BankName,
                            request.Status);
                    }
                }

                // --------------------------------------------------------
                // 6. SOLD LINES -> REAL SALE RECORDS
                // --------------------------------------------------------
                if (request.Direction == "Out")
                {
                    var soldGroups = request.Lines
                        .Where(line => line.Reason == "Sold")
                        .GroupBy(line => line.CustomerId);

                    foreach (var group in soldGroups)
                    {
                        var items = group
                            .Select(line => new CreateSaleItemRequest
                            {
                                ProductId = line.ProductId,
                                Quantity = line.Quantity,
                                UnitPrice = line.UnitPrice
                            })
                            .ToList();

                        await _saleService.CreateSaleAsync(
                            request.BusinessId,
                            group.Key,
                            items,
                            request.PaymentMethod,
                            request.PaymentSource,
                            request.ReferenceNumber,
                            request.Status);
                    }
                }

                // --------------------------------------------------------
                // 7. EVERYTHING SUCCEEDED
                // --------------------------------------------------------
                await transaction.CommitAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
    }
}
