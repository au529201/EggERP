namespace EggERP.Shared.Models
{
    public class RecordEggProductionRequest
    {
        public Guid BusinessId { get; set; }
        public Guid FlockId { get; set; }
        public DateTime ProductionDate { get; set; } = DateTime.Today;
        public int QuantityProduced { get; set; }
        public string? Notes { get; set; }
    }
}