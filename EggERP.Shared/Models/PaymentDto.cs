namespace EggERP.Shared.Models
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid? SaleId { get; set; }
        public Guid? PurchaseId { get; set; }
        public DateTime PaymentDateUtc { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}