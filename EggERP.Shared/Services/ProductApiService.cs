using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IProductApiService
    {
        Task<List<ProductDto>> GetProductsAsync(Guid businessId);
        Task CreateProductAsync(CreateProductRequest request);
        Task<ProductDetailDto?> GetProductByIdAsync(Guid businessId, Guid id);
        Task UpdateProductAsync(UpdateProductRequest request);
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
        public async Task CreateProductAsync(CreateProductRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/products", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task<ProductDetailDto?> GetProductByIdAsync(Guid businessId, Guid id)
        {
            var response = await _http.GetAsync($"api/products/{businessId}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        }
        public async Task UpdateProductAsync(UpdateProductRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/products/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
    }
}