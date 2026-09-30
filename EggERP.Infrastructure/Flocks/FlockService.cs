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

    public async Task<List<StockBoardRowDto>> GetBoardAsync(
        Guid businessId,
        string group)
    {
        // Load active products for this business once.
        var products = await _productRepository
            .GetActiveByBusinessIdAsync(businessId);

        var groupProducts = products
            .Where(p => p.Group == group)
            .ToList();

        if (groupProducts.Count == 0)
        {
            return new List<StockBoardRowDto>();
        }

        var productIds = groupProducts
            .Select(p => p.Id)
            .ToList();

        // Load inventory for all products in this business in one query.
        var inventory = await _inventoryService
            .GetInventoryAsync(businessId);

        var inventoryByProduct = inventory
            .Where(i => productIds.Contains(i.ProductId))
            .ToDictionary(
                i => i.ProductId,
                i => i.QuantityOnHand);

        // Load Bought totals for every displayed product in one query.
        // Purchase.BusinessId ensures the totals belong to this business.
        var boughtByProduct = await (
            from pi in _dbContext.PurchaseItems
            join purchase in _dbContext.Purchases
                on pi.PurchaseId equals purchase.Id
            where purchase.BusinessId == businessId
                  && productIds.Contains(pi.ProductId)
            group pi by pi.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(
                x => x.ProductId,
                x => x.Quantity);

        // Load Sold quantity + sales amount together in one query.
        var soldByProduct = await (
            from si in _dbContext.SaleItems
            join sale in _dbContext.Sales
                on si.SaleId equals sale.Id
            where sale.BusinessId == businessId
                  && productIds.Contains(si.ProductId)
            group si by si.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                TotalSales = g.Sum(x => x.TotalAmount)
            })
            .ToDictionaryAsync(
                x => x.ProductId,
                x => x);

        // Everything below this point is in-memory.
        return groupProducts
            .Select(product =>
            {
                inventoryByProduct.TryGetValue(
                    product.Id,
                    out var available);

                boughtByProduct.TryGetValue(
                    product.Id,
                    out var bought);

                soldByProduct.TryGetValue(
                    product.Id,
                    out var soldData);

                return new StockBoardRowDto
                {
                    ProductId = product.Id,
                    Type = product.Type,
                    Description = product.Description ?? string.Empty,
                    Unit = product.Unit,

                    Available = (int)Math.Round(available),
                    Bought = (int)Math.Round(bought),
                    Sold = soldData is null
                        ? 0
                        : (int)Math.Round(soldData.Quantity),

                    TotalSales = soldData?.TotalSales ?? 0
                };
            })
            .ToList();
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

            // ============================================================
            // 1. VALIDATE EVERY LINE
            // ============================================================
            foreach (var line in request.Lines)
            {
                if (line.Group != "Flock" && line.Group != "Egg")
                {
                    return (false,
                        "Each line must specify Group 'Flock' or 'Egg'.");
                }

                if (line.ProductId == Guid.Empty)
                {
                    return (false,
                        "A product must be selected on every line.");
                }

                if (line.Quantity <= 0)
                {
                    return (false,
                        "Quantity must be greater than zero on every line.");
                }

                var product = await _productRepository.GetByIdAsync(
                    request.BusinessId,
                    line.ProductId);

                if (product is null)
                {
                    return (false,
                        "A selected product was not found in this business.");
                }

                if (product.Group != line.Group)
                {
                    return (false,
                        $"The selected product does not belong to the {line.Group} group.");
                }

                // Bought and Sold are special transaction reasons.
                // They are valid for BOTH Flock and Egg.
                var isBought =
                    request.Direction == "In" &&
                    line.Reason == "Bought";

                var isSold =
                    request.Direction == "Out" &&
                    line.Reason == "Sold";

                // Only ordinary stock reasons need to exist in FlockOptions.
                if (!isBought && !isSold)
                {
                    var validReasons = request.Direction == "In"
                        ? (line.Group == "Flock"
                            ? FlockOptions.FlockInReasons
                            : FlockOptions.EggInReasons)
                        : (line.Group == "Flock"
                            ? FlockOptions.FlockOutReasons
                            : FlockOptions.EggOutReasons);

                    if (!validReasons.Contains(line.Reason))
                    {
                        return (false,
                            $"'{line.Reason}' is not a valid reason for a {line.Group} line.");
                    }
                }

                // Bought validation
                // SupplierId may be null for an Unspecified / casual supplier.
                if (isBought)
                {
                    if (line.UnitCost < 0)
                    {
                        return (false,
                            "Unit cost cannot be negative.");
                    }
                }

                // Sold validation
                // CustomerId may be null for a Walk-in customer.
                if (isSold)
                {
                    if (line.UnitPrice < 0)
                    {
                        return (false,
                            "Unit price cannot be negative.");
                    }
                }

            }

            // ============================================================
            // 2. VALIDATE TOTAL OUTGOING STOCK
            // ============================================================
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
                    var inventory =
                        await _inventoryService.GetByProductIdAsync(
                            request.BusinessId,
                            outgoing.ProductId);

                    var available =
                        inventory?.QuantityOnHand ?? 0;

                    if (outgoing.Quantity > available)
                    {
                        return (false,
                            $"Cannot remove {outgoing.Quantity:N0} unit(s). " +
                            $"Only {available:N0} unit(s) are currently available.");
                    }
                }
            }

            // ============================================================
            // 3. BEGIN DATABASE TRANSACTION
            // ============================================================
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ========================================================
                // 4. DIRECT STOCK MOVEMENTS
                //
                // Bought and Sold are NOT adjusted here because
                // PurchaseService / SaleService already update Inventory.
                // ========================================================
                foreach (var line in request.Lines.Where(l => !IsPaymentLine(l)))
                {
                    var delta =
                        request.Direction == "In"
                            ? (decimal)line.Quantity
                            : -(decimal)line.Quantity;

                    await _inventoryService.AdjustQuantityAsync(
                        request.BusinessId,
                        line.ProductId,
                        delta);
                }

                // ========================================================
                // 5. BOUGHT -> PURCHASE
                // ========================================================
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

                // ========================================================
                // 6. SOLD -> SALE
                // ========================================================
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

                // ========================================================
                // 7. COMMIT
                // ========================================================
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
