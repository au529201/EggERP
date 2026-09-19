namespace EggERP.Domain.Entities;

public class FlockMovement
{
    public Guid Id { get; set; }

    public Guid FlockId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime MovementDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}