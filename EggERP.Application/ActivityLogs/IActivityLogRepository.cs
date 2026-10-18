using EggERP.Domain.Entities;

namespace EggERP.Application.ActivityLogs;

public interface IActivityLogRepository
{
    Task<ActivityLog> AddAsync(ActivityLog log);
    Task<List<ActivityLog>> SearchAsync(ActivityLogFilter filter);
    Task<PagedActivityLogResult> SearchPagedAsync(
    ActivityLogFilter filter,
    int page,
    int pageSize);
}