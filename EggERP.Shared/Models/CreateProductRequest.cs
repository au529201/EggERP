namespace EggERP.Shared.Models
{
    public class CreateProductRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Group { get; set; } = "Flock";
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Unit { get; set; } = "pcs";
        public bool IsActive { get; set; } = true;
    }
}