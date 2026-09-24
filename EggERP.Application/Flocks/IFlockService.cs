namespace EggERP.Application.Flocks;

public class StockBoardRowDto
{
    public Guid ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Bought { get; set; }
    public int Sold { get; set; }
    public decimal TotalSales { get; set; }
}

public class AddStockRequest
{
    public Guid BusinessId { get; set; }
    public Guid ProductId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
}

public class RemoveStockRequest
{
    public Guid BusinessId { get; set; }
    public Guid ProductId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
}

public interface IFlockService
{
    Task<List<StockBoardRowDto>> GetBoardAsync(Guid businessId, string group);
    Task<(bool Succeeded, string? Error)> AddStockAsync(AddStockRequest request);
    Task<(bool Succeeded, string? Error)> RemoveStockAsync(RemoveStockRequest request);
}