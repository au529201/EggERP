namespace EggERP.Shared.Models
{
    public class SaleDto
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime SaleDateUtc { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}