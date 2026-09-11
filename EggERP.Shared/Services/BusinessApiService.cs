using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface IBusinessApiService
    {
        Task<BusinessDto?> GetBusinessAsync(Guid id);
        Task UpdateBusinessAsync(UpdateBusinessRequest request);
    }
    public class BusinessApiService : IBusinessApiService
    {
        private readonly HttpClient _http;
        public BusinessApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<BusinessDto?> GetBusinessAsync(Guid id)
        {
            var response = await _http.GetAsync($"api/business/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BusinessDto>();
        }
        public async Task UpdateBusinessAsync(UpdateBusinessRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/business/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
    }
}