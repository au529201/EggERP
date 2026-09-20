namespace EggERP.Application.ActivityLogs;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime TimestampUtc { get; set; }
}

// BusinessId is always required and injected by the controller from the
// current user, same convention as CreateBusinessUserRequest etc.
public class ActivityLogFilter
{
    public Guid BusinessId { get; set; }
    public string? SearchText { get; set; }     // matches UserName, Action, EntityType, or Details
    public string? EntityType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public interface IActivityLogService
{
    Task LogAsync(Guid businessId, Guid userId, string userName, string action, string entityType, Guid? entityId, string? details);
    Task<List<ActivityLogDto>> SearchAsync(ActivityLogFilter filter);
}