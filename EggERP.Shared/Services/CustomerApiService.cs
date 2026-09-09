using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface ICustomerApiService
    {
        Task<List<CustomerDto>> GetCustomersAsync(Guid businessId);
        Task CreateCustomerAsync(CreateCustomerRequest request);
        Task<CustomerDto?> GetCustomerByIdAsync(Guid businessId, Guid id);
        Task UpdateCustomerAsync(UpdateCustomerRequest request);
        Task DeactivateCustomerAsync(Guid businessId, Guid id);
    }
    public class CustomerApiService : ICustomerApiService
    {
        private readonly HttpClient _http;
        public CustomerApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<CustomerDto>> GetCustomersAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<CustomerDto>>(
                $"api/customers/{businessId}");
            return result ?? new List<CustomerDto>();
        }
        public async Task CreateCustomerAsync(CreateCustomerRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/customers", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid businessId, Guid id)
        {
            var response = await _http.GetAsync($"api/customers/{businessId}/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }
        public async Task UpdateCustomerAsync(UpdateCustomerRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/customers/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeactivateCustomerAsync(Guid businessId, Guid id)
        {
            var response = await _http.DeleteAsync($"api/customers/{businessId}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}