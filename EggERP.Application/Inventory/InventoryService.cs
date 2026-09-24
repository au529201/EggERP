using EggERP.Application.Products;
using EggERP.Domain.Entities;
namespace EggERP.Application.Inventory;
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public InventoryService(IInventoryRepository inventoryRepository, IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public Task<List<EggERP.Domain.Entities.Inventory>> GetInventoryAsync(Guid businessId)
    {
        return _inventoryRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(Guid businessId, Guid productId)
    {
        return _inventoryRepository.GetByProductIdAsync(businessId, productId);
    }
    public Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta)
    {
        return _inventoryRepository.AdjustQuantityAsync(businessId, productId, delta);
    }

    public async Task<List<InventoryBoardRowDto>> GetBoardAsOfDateAsync(Guid businessId, DateTime asOfDate)
    {
        var products = await _productRepository.GetActiveByBusinessIdAsync(businessId);
        var inventory = await _inventoryRepository.GetByBusinessIdAsync(businessId);

        var boughtByProduct = await _inventoryRepository.GetBoughtQuantitiesAsOfDateAsync(businessId, asOfDate);
        var soldByProduct = await _inventoryRepository.GetSoldQuantitiesAsOfDateAsync(businessId, asOfDate);

        return products.Select(p =>
        {
            var invRecord = inventory.FirstOrDefault(i => i.ProductId == p.Id);
            return new InventoryBoardRowDto
            {
                ProductId = p.Id,
                Type = p.Type,
                Description = p.Description ?? string.Empty,
                Group = p.Group,
                Unit = p.Unit,
                SellingPrice = p.SellingPrice,
                Available = (int)Math.Floor(invRecord?.QuantityOnHand ?? 0),
                ReorderLevel = (int)Math.Floor(invRecord?.ReorderLevel ?? 0),
                BoughtAsOfDate = (int)Math.Round(boughtByProduct.GetValueOrDefault(p.Id, 0)),
                SoldAsOfDate = (int)Math.Round(soldByProduct.GetValueOrDefault(p.Id, 0))
            };
        }).ToList();
    }
}