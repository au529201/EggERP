using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IActivityLogApiService
    {
        Task<List<ActivityLogDto>> SearchAsync(string? search, string? entityType, DateTime? startDate, DateTime? endDate);
    }

    public class ActivityLogApiService : IActivityLogApiService
    {
        private readonly HttpClient _http;

        public ActivityLogApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ActivityLogDto>> SearchAsync(string? search, string? entityType, DateTime? startDate, DateTime? endDate)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(entityType)) query.Add($"entityType={Uri.EscapeDataString(entityType)}");
            if (startDate.HasValue) query.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) query.Add($"endDate={endDate:yyyy-MM-dd}");

            var url = "api/activitylogs" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            var result = await _http.GetFromJsonAsync<List<ActivityLogDto>>(url);
            return result ?? new List<ActivityLogDto>();
        }
    }
}