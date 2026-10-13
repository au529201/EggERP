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
            if (businessId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "A valid Business ID is required to load sales.");
            }

            var response = await _http.GetAsync($"api/sales/{businessId}");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException(
                    $"Sales API failed. " +
                    $"HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                    $"Requested BusinessId: {businessId}. " +
                    $"Response: {content[..Math.Min(content.Length, 300)]}");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (mediaType is null ||
                !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Sales API returned unexpected content type: " +
                    $"{mediaType ?? "unknown"}. " +
                    $"Requested BusinessId: {businessId}.");
            }

            return await response.Content.ReadFromJsonAsync<List<SaleDto>>()
                   ?? new List<SaleDto>();
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