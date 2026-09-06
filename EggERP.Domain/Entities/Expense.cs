namespace EggERP.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public DateTime ExpenseDateUtc { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public string Status { get; set; } = "Paid";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}