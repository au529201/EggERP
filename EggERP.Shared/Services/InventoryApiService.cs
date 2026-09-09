using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface IInventoryApiService
    {
        Task<List<InventoryDto>> GetInventoryAsync(Guid businessId);
        Task<InventoryDto> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta);
    }
    public class InventoryApiService : IInventoryApiService
    {
        private readonly HttpClient _http;
        public InventoryApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<InventoryDto>> GetInventoryAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<InventoryDto>>(
                $"api/inventory/{businessId}");
            return result ?? new List<InventoryDto>();
        }
        public async Task<InventoryDto> AdjustQuantityAsync(Guid businessId, Guid productId, decimal delta)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/inventory/{businessId}/product/{productId}/adjust",
                new { delta });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<InventoryDto>())!;
        }
    }
}