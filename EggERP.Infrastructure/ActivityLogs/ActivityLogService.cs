using EggERP.Application.ActivityLogs;
using EggERP.Domain.Entities;

namespace EggERP.Infrastructure.ActivityLogs;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogService(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public async Task LogAsync(Guid businessId, Guid userId, string userName, string action, string entityType, Guid? entityId, string? details)
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            TimestampUtc = DateTime.UtcNow
        };

        await _activityLogRepository.AddAsync(log);
    }

    public async Task<List<ActivityLogDto>> SearchAsync(ActivityLogFilter filter)
    {
        var logs = await _activityLogRepository.SearchAsync(filter);

        return logs.Select(l => new ActivityLogDto
        {
            Id = l.Id,
            UserName = l.UserName,
            Action = l.Action,
            EntityType = l.EntityType,
            Details = l.Details,
            TimestampUtc = l.TimestampUtc
        }).ToList();
    }
}