namespace EggERP.Shared.Models
{
    public class AddStockRequest
    {
        public Guid BusinessId { get; set; }
        public Guid ProductId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Today;
        public string? Notes { get; set; }
    }
}