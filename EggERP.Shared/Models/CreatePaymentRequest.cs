namespace EggERP.Shared.Models
{
    public class CreatePaymentRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? SaleId { get; set; }
        public Guid? PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Completed";
    }
}