using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IProductApiService
    {
        Task<List<ProductDto>> GetProductsAsync(Guid businessId);
    }

    public class ProductApiService : IProductApiService
    {
        private readonly HttpClient _http;

        public ProductApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductDto>> GetProductsAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<ProductDto>>(
                $"api/products/{businessId}");

            return result ?? new List<ProductDto>();
        }
    }
}