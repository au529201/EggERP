
using EggERP.Application.Products;
using EggERP.Domain.Entities;

namespace EggERP.Application.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public Task<List<EggERP.Domain.Entities.Inventory>>
        GetInventoryAsync(Guid businessId)
    {
        EnsureBusinessId(businessId);

        return _inventoryRepository.GetByBusinessIdAsync(businessId);
    }

    public Task<EggERP.Domain.Entities.Inventory?>
        GetByProductIdAsync(Guid businessId, Guid productId)
    {
        EnsureBusinessId(businessId);
        EnsureProductId(productId);

        return _inventoryRepository.GetByProductIdAsync(
            businessId, productId);
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        AdjustQuantityAsync(
            Guid businessId,
            Guid productId,
            decimal delta)
    {
        EnsureBusinessId(businessId);
        EnsureProductId(productId);

        if (delta == 0)
        {
            throw new InvalidOperationException(
                "Stock adjustment quantity cannot be zero.");
        }

        var product = await _productRepository.GetByIdAsync(
            businessId, productId);

        if (product is null)
        {
            throw new InvalidOperationException(
                "The selected product does not belong to this business.");
        }

        if (!product.IsActive)
        {
            throw new InvalidOperationException(
                "The selected product is inactive. " +
                "Reactivate it before processing stock transactions.");
        }

        return await _inventoryRepository.AdjustQuantityAsync(
            businessId, productId, delta);
    }

    public async Task<EggERP.Domain.Entities.Inventory>
        SetThresholdsAsync(
            Guid businessId,
            Guid productId,
            decimal lowThreshold,
            decimal? highThreshold)
    {
        EnsureBusinessId(businessId);
        EnsureProductId(productId);

        var product = await _productRepository.GetByIdAsync(
            businessId, productId);

        if (product is null)
        {
            throw new InvalidOperationException(
                "The selected product does not belong to this business.");
        }

        return await _inventoryRepository.SetThresholdsAsync(
            businessId,
            productId,
            lowThreshold,
            highThreshold);
    }

    public async Task<List<InventoryBoardRowDto>>
        GetBoardAsOfDateAsync(Guid businessId, DateTime asOfDate)
    {
        EnsureBusinessId(businessId);

        var products =
            await _productRepository.GetActiveByBusinessIdAsync(businessId);

        var inventory =
            await _inventoryRepository.GetByBusinessIdAsync(businessId);

        var inventoryByProduct = inventory
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.First());

        var boughtByProduct =
            await _inventoryRepository.GetBoughtQuantitiesAsOfDateAsync(
                businessId, asOfDate);

        var soldByProduct =
            await _inventoryRepository.GetSoldQuantitiesAsOfDateAsync(
                businessId, asOfDate);

        return products.Select(p =>
        {
            inventoryByProduct.TryGetValue(p.Id, out var inv);

            return new InventoryBoardRowDto
            {
                ProductId = p.Id,
                Type = p.Type,
                Description = p.Description ?? string.Empty,
                Group = p.Group,
                Unit = p.Unit,
                SellingPrice = p.SellingPrice,

                Available = (int)Math.Floor(
                    inv?.QuantityOnHand ?? 0),

                ReorderLevel = (int)Math.Floor(
                    inv?.ReorderLevel ?? 0),

                HighStockThreshold = inv?.HighStockThreshold,

                BoughtAsOfDate = (int)Math.Round(
                    boughtByProduct.GetValueOrDefault(p.Id, 0)),

                SoldAsOfDate = (int)Math.Round(
                    soldByProduct.GetValueOrDefault(p.Id, 0))
            };
        }).ToList();
    }

    private static void EnsureBusinessId(Guid businessId)
    {
        if (businessId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid Business ID is required.");
        }
    }

    private static void EnsureProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid Product ID is required.");
        }
    }
}
