namespace EggERP.Domain.Entities;

public class EggMovement
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? FlockId { get; set; }

    public DateTime MovementDate { get; set; }

    public string Direction { get; set; } = string.Empty; // "In" or "Out"

    public string Reason { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}