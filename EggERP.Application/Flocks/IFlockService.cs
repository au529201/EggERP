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

public class StockTransactionLineRequest
{
    public string Group { get; set; } = "Flock";
    public Guid ProductId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class ProcessStockTransactionRequest
{
    public Guid BusinessId { get; set; }
    public string Direction { get; set; } = "In";
    public List<StockTransactionLineRequest> Lines { get; set; } = new();
    public DateTime MovementDate { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? PaymentSource { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string Status { get; set; } = "Paid";
}

public interface IFlockService
{
    Task<List<StockBoardRowDto>> GetBoardAsync(Guid businessId, string group);
    Task<(bool Succeeded, string? Error)> AddStockAsync(AddStockRequest request);
    Task<(bool Succeeded, string? Error)> RemoveStockAsync(RemoveStockRequest request);

    // New unified multi-line entry point used by AddStock.razor / RemoveStock.razor.
    // Lines with Reason == "Bought"/"Sold" are grouped by Supplier/Customer and turned
    // into real Purchase/Sale records; every other reason adjusts Inventory directly.
    Task<(bool Succeeded, string? Error)> ProcessTransactionAsync(ProcessStockTransactionRequest request);
}