using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IFlockApiService
    {
        Task<List<FlockDto>> GetFlocksAsync();
        Task<string?> CreateFlockAsync(CreateFlockRequest request);
        Task<string?> RecordMovementAsync(RecordFlockMovementRequest request);
    }

    public class FlockApiService : IFlockApiService
    {
        private readonly HttpClient _http;

        public FlockApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<FlockDto>> GetFlocksAsync()
        {
            var result = await _http.GetFromJsonAsync<List<FlockDto>>("api/flocks");
            return result ?? new List<FlockDto>();
        }

        public async Task<string?> CreateFlockAsync(CreateFlockRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/flocks", request);
            if (response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> RecordMovementAsync(RecordFlockMovementRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/flocks/movements", request);
            if (response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}