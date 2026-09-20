namespace EggERP.Domain.Entities;

public class Flock
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string BirdType { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public DateTime AcquisitionDate { get; set; }

    public int InitialCount { get; set; }

    public int CurrentCount { get; set; }

    public decimal? AcquisitionCost { get; set; }
    public Guid? LinkedProductId { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}