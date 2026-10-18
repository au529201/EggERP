    using EggERP.Application;
    using EggERP.Application.Flocks;
    using EggERP.Application.Inventory;
    using EggERP.Application.Products;
    using EggERP.Application.Purchases;
    using EggERP.Application.Sales;
    using EggERP.Infrastructure.Persistence;
    using Microsoft.EntityFrameworkCore;
    using EggERP.Application.Transactions;
    namespace EggERP.Infrastructure.Flocks;

    public class FlockService : IFlockService
    {
        private readonly IProductRepository _productRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPurchaseService _purchaseService;
        private readonly ISaleService _saleService;
        private readonly EggERPDbContext _dbContext;
       private readonly ITransactionManager _transactionManager;
    public FlockService(
            IProductRepository productRepository,
            IInventoryService inventoryService,
            IPurchaseService purchaseService,
            ISaleService saleService,
            EggERPDbContext dbContext,
            ITransactionManager transactionManager)
    {
            _transactionManager = transactionManager;
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


    public async Task<(bool Succeeded, string? Error)>
        ProcessTransactionAsync(ProcessStockTransactionRequest request)
    {
        if (request.BusinessId == Guid.Empty)
            return (false, "A valid business is required.");

        if (request.Lines is null || request.Lines.Count == 0)
            return (false, "At least one stock line is required.");

        if (request.Direction != "In" && request.Direction != "Out")
            return (false, "Invalid stock transaction direction.");

        if (request.Lines.Count > 100)
            return (false, "Maximum of 100 lines per transaction.");

        bool IsPaymentLine(StockTransactionLineRequest line) =>
            (request.Direction == "In" && line.Reason == "Bought") ||
            (request.Direction == "Out" && line.Reason == "Sold");

        var hasPaymentLine = request.Lines.Any(IsPaymentLine);

        try
        {
            if (hasPaymentLine)
            {
                if (request.Status != "Paid" &&
                    request.Status != "Pending")
                {
                    return (false, "Status must be Paid or Pending.");
                }

                PaymentValidation.EnsureValid(
                    request.PaymentMethod,
                    request.ReferenceNumber,
                    request.PaymentSource);
            }

            // Validate duplicate products across all lines.
            // A product may appear only once per transaction.
            var duplicateProduct = request.Lines
                .GroupBy(l => l.ProductId)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateProduct is not null)
            {
                return (false,
                    "A product cannot appear more than once " +
                    "in the same stock transaction.");
            }

            // Validate every product before writing anything.
            foreach (var line in request.Lines)
            {
                if (line.Group != "Flock" && line.Group != "Egg")
                {
                    return (false,
                        "Each line must belong to Flock or Egg.");
                }

                if (line.ProductId == Guid.Empty)
                {
                    return (false,
                        "Select a valid product on every line.");
                }

                if (line.Quantity <= 0 || line.Quantity > 9999)
                {
                    return (false,
                        "Quantity must be between 1 and 9,999.");
                }

                var product = await _productRepository.GetByIdAsync(
                    request.BusinessId,
                    line.ProductId);

                if (product is null)
                {
                    return (false,
                        "A selected product does not belong " +
                        "to the current business.");
                }

                if (!product.IsActive)
                {
                    return (false,
                        $"Product '{product.Type}' is inactive. " +
                        "Reactivate it before continuing.");
                }

                if (product.Group != line.Group)
                {
                    return (false,
                        $"Product '{product.Type}' does not " +
                        $"belong to the {line.Group} group.");
                }

                var isBought =
                    request.Direction == "In" &&
                    line.Reason == "Bought";

                var isSold =
                    request.Direction == "Out" &&
                    line.Reason == "Sold";

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
                            $"'{line.Reason}' is not a valid " +
                            $"{line.Group} stock reason.");
                    }
                }

                if (isBought && line.UnitCost < 0)
                {
                    return (false,
                        "Unit cost cannot be negative.");
                }

                if (isSold && line.UnitPrice < 0)
                {
                    return (false,
                        "Unit price cannot be negative.");
                }
            }

            // Check stock before beginning the transaction.
            // The repository performs a second atomic check
            // when actually deducting inventory.
            if (request.Direction == "Out")
            {
                foreach (var line in request.Lines)
                {
                    var inventory =
                        await _inventoryService.GetByProductIdAsync(
                            request.BusinessId,
                            line.ProductId);

                    var available =
                        inventory?.QuantityOnHand ?? 0;

                    if (line.Quantity > available)
                    {
                        var product =
                            await _productRepository.GetByIdAsync(
                                request.BusinessId,
                                line.ProductId);

                        return (false,
                            $"Insufficient stock for " +
                            $"'{product?.Description ?? product?.Type}'. " +
                            $"Available: {available:N0}; " +
                            $"Requested: {line.Quantity:N0}.");
                    }
                }
            }

            // All stock movements and Purchase/Sale records
            // must succeed before committing the transaction.
            try
            {
                await _transactionManager.ExecuteAsync(async () =>
                {
                    // Direct inventory movements.
                    foreach (var line in request.Lines
                    .Where(l => !IsPaymentLine(l)))
                {
                    decimal delta = request.Direction == "In"
                        ? line.Quantity
                        : -line.Quantity;

                    await _inventoryService.AdjustQuantityAsync(
                        request.BusinessId,
                        line.ProductId,
                        delta);
                }

                // Bought lines create real Purchases.
                // PurchaseService owns their stock additions.
                if (request.Direction == "In")
                {
                    var boughtGroups = request.Lines
                        .Where(l => l.Reason == "Bought")
                        .GroupBy(l => l.SupplierId);

                    foreach (var group in boughtGroups)
                    {
                        var items = group.Select(line =>
                            new CreatePurchaseItemRequest
                            {
                                ProductId = line.ProductId,
                                Quantity = line.Quantity,
                                UnitCost = line.UnitCost
                            }).ToList();

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

                // Sold lines create real Sales.
                // SaleService owns their stock deductions.
                if (request.Direction == "Out")
                {
                    var soldGroups = request.Lines
                        .Where(l => l.Reason == "Sold")
                        .GroupBy(l => l.CustomerId);

                    foreach (var group in soldGroups)
                    {
                        var items = group.Select(line =>
                            new CreateSaleItemRequest
                            {
                                ProductId = line.ProductId,
                                Quantity = line.Quantity,
                                UnitPrice = line.UnitPrice
                            }).ToList();

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
                });

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false,
                    $"Stock transaction failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return (false,
                $"Stock transaction validation failed: {ex.Message}");
        }
    }
}