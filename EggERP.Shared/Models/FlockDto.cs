namespace EggERP.Shared.Models
{
    public class FlockDto
    {
        public Guid Id { get; set; }
        public string BirdType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime AcquisitionDate { get; set; }
        public int InitialCount { get; set; }
        public int CurrentCount { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public Guid? LinkedProductId { get; set; }
        public string? LinkedProductName { get; set; }
    }
}