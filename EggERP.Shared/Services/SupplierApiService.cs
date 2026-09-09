using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface ISupplierApiService
    {
        Task<List<SupplierDto>> GetSuppliersAsync(Guid businessId);
        Task CreateSupplierAsync(CreateSupplierRequest request);
        Task<SupplierDto?> GetSupplierByIdAsync(Guid businessId, Guid id);
        Task UpdateSupplierAsync(UpdateSupplierRequest request);
        Task DeactivateSupplierAsync(Guid businessId, Guid id);
    }
    public class SupplierApiService : ISupplierApiService
    {
        private readonly HttpClient _http;
        public SupplierApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<SupplierDto>> GetSuppliersAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<SupplierDto>>(
                $"api/suppliers/{businessId}");
            return result ?? new List<SupplierDto>();
        }
        public async Task CreateSupplierAsync(CreateSupplierRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/suppliers", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task<SupplierDto?> GetSupplierByIdAsync(Guid businessId, Guid id)
        {
            var response = await _http.GetAsync($"api/suppliers/{businessId}/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SupplierDto>();
        }
        public async Task UpdateSupplierAsync(UpdateSupplierRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/suppliers/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeactivateSupplierAsync(Guid businessId, Guid id)
        {
            var response = await _http.DeleteAsync($"api/suppliers/{businessId}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}