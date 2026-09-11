namespace EggERP.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid? CustomerId { get; set; }

    public DateTime SaleDateUtc { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public string? PaymentSource { get; set; }

    public string? ReferenceNumber { get; set; }

    public string Status { get; set; } = "Completed";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}