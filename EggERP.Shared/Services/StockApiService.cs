using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IStockApiService
    {
        Task<string?> ProcessAsync(ProcessStockTransactionRequest request);
    }

    public class StockApiService : IStockApiService
    {
        private readonly HttpClient _http;

        public StockApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> ProcessAsync(ProcessStockTransactionRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/stock/process", request);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }
    }
}