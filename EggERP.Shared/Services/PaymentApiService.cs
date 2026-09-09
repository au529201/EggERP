using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface IPaymentApiService
    {
        Task<List<PaymentDto>> GetPaymentsAsync(Guid businessId);
        Task CreatePaymentAsync(CreatePaymentRequest request);
    }
    public class PaymentApiService : IPaymentApiService
    {
        private readonly HttpClient _http;
        public PaymentApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<PaymentDto>> GetPaymentsAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<PaymentDto>>(
                $"api/payments/{businessId}");
            return result ?? new List<PaymentDto>();
        }
        public async Task CreatePaymentAsync(CreatePaymentRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/payments", request);
            response.EnsureSuccessStatusCode();
        }
    }
}