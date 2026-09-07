using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface ICategoryApiService
    {
        Task<List<CategoryDto>> GetCategoriesAsync(Guid businessId);
        Task CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryDto?> GetCategoryByIdAsync(Guid businessId, Guid id);
        Task UpdateCategoryAsync(UpdateCategoryRequest request);
        Task DeactivateCategoryAsync(Guid businessId, Guid id);
    }
    public class CategoryApiService : ICategoryApiService
    {
        private readonly HttpClient _http;
        public CategoryApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<CategoryDto>> GetCategoriesAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<CategoryDto>>(
                $"api/categories/{businessId}");
            return result ?? new List<CategoryDto>();
        }
        public async Task CreateCategoryAsync(CreateCategoryRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/categories", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid businessId, Guid id)
        {
            var response = await _http.GetAsync($"api/categories/{businessId}/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryDto>();
        }
        public async Task UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/categories/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeactivateCategoryAsync(Guid businessId, Guid id)
        {
            var response = await _http.DeleteAsync($"api/categories/{businessId}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}