using EggERP.Domain.Entities;
namespace EggERP.Application.Inventory;
public interface IInventoryService
{
    Task<List<EggERP.Domain.Entities.Inventory>> GetInventoryAsync(Guid businessId);
    Task<EggERP.Domain.Entities.Inventory?> GetByProductIdAsync(Guid businessId, Guid productId);
    Task<EggERP.Domain.Entities.Inventory> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta);
    Task<List<InventoryBoardRowDto>> GetBoardAsOfDateAsync(Guid businessId, DateTime asOfDate);
}

public class InventoryBoardRowDto
{
    public Guid ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int Available { get; set; }
    public int ReorderLevel { get; set; }
    public int BoughtAsOfDate { get; set; }
    public int SoldAsOfDate { get; set; }
}