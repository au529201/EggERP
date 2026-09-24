namespace EggERP.Shared.Models
{
    public class PurchaseDto
    {
        public Guid Id { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime PurchaseDateUtc { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? BankName { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}