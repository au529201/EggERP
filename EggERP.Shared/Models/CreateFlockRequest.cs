namespace EggERP.Shared.Models
{
    public class CreateFlockRequest
    {
        public Guid BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string? Breed { get; set; }
        public DateTime PlacementDate { get; set; } = DateTime.Today;
        public decimal? AcquisitionCost { get; set; }
        public string? Notes { get; set; }
        public Guid LinkedProductId { get; set; }
        public int InitialQuantity { get; set; }
        public string InitialSource { get; set; } = "Bought";
    }
}