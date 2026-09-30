using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface ISaleApiService
    {
        Task<List<SaleDto>> GetSalesAsync(Guid businessId);
        Task CreateSaleAsync(CreateSaleRequest request);

        Task MarkAsPaidAsync(
            Guid businessId,
            Guid saleId,
            string paymentMethod,
            string? paymentSource,
            string? referenceNumber);
    }

    public class SaleApiService : ISaleApiService
    {
        private readonly HttpClient _http;

        public SaleApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<SaleDto>> GetSalesAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<SaleDto>>(
                $"api/sales/{businessId}");

            return result ?? new List<SaleDto>();
        }

        public async Task CreateSaleAsync(CreateSaleRequest request)
        {
            var response = await _http.PostAsJsonAsync(
                "api/sales",
                request);

            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAsPaidAsync(
            Guid businessId,
            Guid saleId,
            string paymentMethod,
            string? paymentSource,
            string? referenceNumber)
        {
            var request = new
            {
                PaymentMethod = paymentMethod,
                PaymentSource = paymentSource,
                ReferenceNumber = referenceNumber
            };

            var response = await _http.PutAsJsonAsync(
                $"api/sales/{businessId}/{saleId}/mark-paid",
                request);

            response.EnsureSuccessStatusCode();
        }
    }
}