
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

    public Task<List<ActivityLog>> SearchAsync(
        ActivityLogFilter filter)
    {
        return BuildQuery(filter)
            .OrderByDescending(l => l.TimestampUtc)
            .ThenByDescending(l => l.Id)
            .Take(500)
            .ToListAsync();
    }

    public async Task<PagedActivityLogResult> SearchPagedAsync(
        ActivityLogFilter filter,
        int page,
        int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = BuildQuery(filter);

        var totalCount = await query.CountAsync();

        var skip = ((long)page - 1) * pageSize;

        var items = skip > int.MaxValue
            ? new List<ActivityLog>()
            : await query
                .OrderByDescending(l => l.TimestampUtc)
                .ThenByDescending(l => l.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .ToListAsync();

        return new PagedActivityLogResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private IQueryable<ActivityLog> BuildQuery(
        ActivityLogFilter filter)
    {
        if (filter.BusinessId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid business ID is required.",
                nameof(filter));
        }

        var query = _dbContext.ActivityLogs
            .AsNoTracking()
            .Where(l => l.BusinessId == filter.BusinessId)

            // Platform-only administrative events must never
            // appear in business-facing activity history.
            .Where(l => !(
                l.EntityType == "Business" &&
                (
                    l.Action == "Suspended Business" ||
                    l.Action == "Reactivated Business"
                )
            ));

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            query = query.Where(l =>
                l.EntityType == filter.EntityType);
        }

        if (filter.StartDate.HasValue)
        {
            var start = filter.StartDate.Value.Date;

            query = query.Where(l =>
                l.TimestampUtc >= start);
        }

        if (filter.EndDate.HasValue)
        {
            var endExclusive =
                filter.EndDate.Value.Date.AddDays(1);

            query = query.Where(l =>
                l.TimestampUtc < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var term = filter.SearchText.Trim();

            query = query.Where(l =>
                l.UserName.Contains(term) ||
                l.Action.Contains(term) ||
                l.EntityType.Contains(term) ||
                (l.Details != null &&
                 l.Details.Contains(term)));
        }

        return query;
    }
}
