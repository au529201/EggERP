namespace EggERP.Shared.Models
{
    public class ExpenseDto
    {
        public Guid Id { get; set; }
        public DateTime ExpenseDateUtc { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}