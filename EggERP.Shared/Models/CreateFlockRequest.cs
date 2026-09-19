namespace EggERP.Shared.Models
{
    public class CreateFlockRequest
    {
        public Guid BusinessId { get; set; }
        public string BirdType { get; set; } = "Chicken";
        public string Source { get; set; } = "Bought";
        public DateTime AcquisitionDate { get; set; } = DateTime.Today;
        public int InitialCount { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public string? Notes { get; set; }
    }
}