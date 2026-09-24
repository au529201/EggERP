using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IEggApiService
    {
        Task<List<StockBoardRowDto>> GetBoardAsync();
        Task<string?> AddStockAsync(AddStockRequest request);
        Task<string?> RemoveStockAsync(RemoveStockRequest request);
    }

    public class EggApiService : IEggApiService
    {
        private readonly HttpClient _http;

        public EggApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<StockBoardRowDto>> GetBoardAsync()
        {
            var result = await _http.GetFromJsonAsync<List<StockBoardRowDto>>("api/eggs/board");
            return result ?? new List<StockBoardRowDto>();
        }

        public async Task<string?> AddStockAsync(AddStockRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/eggs/add", request);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> RemoveStockAsync(RemoveStockRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/eggs/remove", request);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }
    }
}