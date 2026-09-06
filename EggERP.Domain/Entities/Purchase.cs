namespace EggERP.Domain.Entities;

public class Purchase
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid? SupplierId { get; set; }

    public DateTime PurchaseDateUtc { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Completed";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}