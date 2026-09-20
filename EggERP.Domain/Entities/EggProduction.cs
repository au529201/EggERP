namespace EggERP.Domain.Entities;

public class EggProduction
{
    public Guid Id { get; set; }

    public Guid FlockId { get; set; }

    public DateTime ProductionDate { get; set; }

    public int QuantityProduced { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}