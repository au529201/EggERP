namespace EggERP.Shared.Models
{
    public class UpdateProductRequest
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}