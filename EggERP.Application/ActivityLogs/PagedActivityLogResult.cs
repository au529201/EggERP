
using EggERP.Domain.Entities;

namespace EggERP.Application.ActivityLogs;

public class PagedActivityLogResult
{
    public List<ActivityLog> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
