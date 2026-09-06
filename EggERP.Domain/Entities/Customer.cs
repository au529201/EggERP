namespace EggERP.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}