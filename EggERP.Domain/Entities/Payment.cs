namespace EggERP.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid? SaleId { get; set; }

    public Guid? PurchaseId { get; set; }

    public DateTime PaymentDateUtc { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public string Status { get; set; } = "Completed";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}