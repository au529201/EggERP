
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

            using var response =
                await _http.GetAsync($"api/sales/{businessId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    await GetErrorMessageAsync(
                        response,
                        "Unable to load sales."));
            }

            var mediaType =
                response.Content.Headers.ContentType?.MediaType;

            if (mediaType is null ||
                !mediaType.Contains(
                    "json",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Sales API returned an unexpected response.");
            }

            return await response.Content
                .ReadFromJsonAsync<List<SaleDto>>()
                ?? new List<SaleDto>();
        }

        public async Task CreateSaleAsync(CreateSaleRequest request)
        {
            using var response = await _http.PostAsJsonAsync(
                "api/sales",
                request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    await GetErrorMessageAsync(
                        response,
                        "Unable to complete sale."));
            }
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

            using var response = await _http.PutAsJsonAsync(
                $"api/sales/{businessId}/{saleId}/mark-paid",
                request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    await GetErrorMessageAsync(
                        response,
                        "Unable to mark sale as paid."));
            }
        }

        private static async Task<string> GetErrorMessageAsync(
            HttpResponseMessage response,
            string fallback)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return "Your session has expired. Please sign in again.";

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return "You do not have permission to perform this action.";

            string content;

            try
            {
                content = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return fallback;
            }

            if (string.IsNullOrWhiteSpace(content))
                return fallback;

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                {
                    var value = root.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var name in new[]
                    {
                        "message",
                        "error",
                        "detail",
                        "title"
                    })
                    {
                        foreach (var property in root.EnumerateObject())
                        {
                            if (!property.Name.Equals(
                                name,
                                StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (property.Value.ValueKind ==
                                JsonValueKind.String)
                            {
                                var value = property.Value.GetString();

                                if (!string.IsNullOrWhiteSpace(value))
                                    return value;
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // The API returned a non-JSON response.
            }

            // Do not expose raw HTML, server stack traces,
            // or other internal response details to users.
            if (content.TrimStart().StartsWith("<") ||
                content.Contains('\n') ||
                content.Length > 300)
            {
                return fallback;
            }

            return content.Trim().Trim('"');
        }
    }
}
