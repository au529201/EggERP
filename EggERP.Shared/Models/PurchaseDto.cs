namespace EggERP.Shared.Models
{
    public class PurchaseDto
    {
        public Guid Id { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime PurchaseDateUtc { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}