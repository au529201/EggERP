using EggERP.Application.ActivityLogs;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.ActivityLogs;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly EggERPDbContext _dbContext;

    public ActivityLogRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityLog> AddAsync(ActivityLog log)
    {
        _dbContext.ActivityLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        return log;
    }

    public Task<List<ActivityLog>> SearchAsync(ActivityLogFilter filter)
    {
        var query = _dbContext.ActivityLogs
            .Where(l => l.BusinessId == filter.BusinessId);

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            query = query.Where(l => l.EntityType == filter.EntityType);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(l => l.TimestampUtc >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            // Include the whole end day.
            var endOfDay = filter.EndDate.Value.Date.AddDays(1);
            query = query.Where(l => l.TimestampUtc < endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var term = filter.SearchText.Trim();
            query = query.Where(l =>
                l.UserName.Contains(term) ||
                l.Action.Contains(term) ||
                l.EntityType.Contains(term) ||
                (l.Details != null && l.Details.Contains(term)));
        }

        return query
            .OrderByDescending(l => l.TimestampUtc)
            .Take(500) // simple MVP cap; pagination can come later if needed
            .ToListAsync();
    }
}