namespace EggERP.Domain.Entities;

public class Flock
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string? Breed { get; set; }

    public Guid? LinkedProductId { get; set; }

    public DateTime PlacementDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public decimal? AcquisitionCost { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}