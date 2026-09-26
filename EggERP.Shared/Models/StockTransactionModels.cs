namespace EggERP.Shared.Models
{
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
        public string Direction { get; set; } = "In"; // "In" = Add Stock, "Out" = Remove Stock
        public List<StockTransactionLineRequest> Lines { get; set; } = new();
        public DateTime MovementDate { get; set; } = DateTime.Today;
        public string? Notes { get; set; }

        // Only meaningful/required if at least one line has Reason == Bought or Sold.
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? BankName { get; set; }
        public string Status { get; set; } = "Paid";
    }
}