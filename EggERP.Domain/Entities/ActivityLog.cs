namespace EggERP.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;      // e.g. "Created Sale"

    public string EntityType { get; set; } = string.Empty;  // e.g. "Sale"

    public Guid? EntityId { get; set; }

    public string? Details { get; set; }

    public DateTime TimestampUtc { get; set; }
}