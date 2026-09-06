namespace EggERP.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid ProductId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReorderLevel { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}