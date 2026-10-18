
using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services;

public interface IActivityLogApiService
{
    Task<List<ActivityLogDto>> SearchAsync(
        string? search,
        string? entityType,
        DateTime? startDate,
        DateTime? endDate);

    Task<PagedActivityLogDto> SearchPagedAsync(
        string? search,
        string? entityType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);
}

public class ActivityLogApiService : IActivityLogApiService
{
    private readonly HttpClient _http;

    public ActivityLogApiService(HttpClient http)
    {
        _http = http;
    }

    private static string BuildQuery(
        string? search,
        string? entityType,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(search)}");
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query.Add(
                $"entityType={Uri.EscapeDataString(entityType)}");
        }

        if (startDate.HasValue)
        {
            query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        }

        if (endDate.HasValue)
        {
            query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        }

        return string.Join("&", query);
    }

    public async Task<List<ActivityLogDto>> SearchAsync(
        string? search,
        string? entityType,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = BuildQuery(
            search, entityType, startDate, endDate);

        var url = "api/activitylogs" +
            (query.Length > 0 ? "?" + query : "");

        return await _http.GetFromJsonAsync<List<ActivityLogDto>>(
            url) ?? new();
    }

    public async Task<PagedActivityLogDto> SearchPagedAsync(
        string? search,
        string? entityType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize)
    {
        var query = BuildQuery(
            search, entityType, startDate, endDate);

        var paging = $"page={page}&pageSize={pageSize}";

        var url = "api/activitylogs/paged?" +
            (query.Length > 0 ? query + "&" : "") +
            paging;

        return await _http.GetFromJsonAsync<PagedActivityLogDto>(
            url) ?? new();
    }
}
