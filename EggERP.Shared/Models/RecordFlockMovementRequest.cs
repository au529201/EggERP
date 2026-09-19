namespace EggERP.Shared.Models
{
    public class RecordFlockMovementRequest
    {
        public Guid BusinessId { get; set; }
        public Guid FlockId { get; set; }
        public string Reason { get; set; } = "Sold";
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Today;
        public string? Notes { get; set; }
    }
}