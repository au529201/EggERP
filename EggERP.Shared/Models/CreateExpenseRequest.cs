namespace EggERP.Shared.Models
{
    public class CreateExpenseRequest
    {
        public Guid BusinessId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = "Paid";
    }
}