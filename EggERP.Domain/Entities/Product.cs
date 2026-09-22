namespace EggERP.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Group { get; set; } = "Flock"; // "Flock" or "Egg"

    public string Type { get; set; } = string.Empty; // Chicken, Duck, Quail, Geese, Turkey, Other

    public string Name { get; set; } = string.Empty; // auto-set: "{Type} - {Description}"

    public string? Description { get; set; }

    public decimal CostPrice { get; set; }

    public decimal SellingPrice { get; set; }

    public string Unit { get; set; } = "pcs";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}