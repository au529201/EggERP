namespace EggERP.Domain.Entities;

public class PurchaseItem
{
    public Guid Id { get; set; }

    public Guid PurchaseId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }
}