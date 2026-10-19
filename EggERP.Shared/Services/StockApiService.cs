
using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IStockApiService
    {
        Task<string?> ProcessAsync(
            ProcessStockTransactionRequest request,
            string csrfToken);
    }

    public class StockApiService : IStockApiService
    {
        private readonly HttpClient _http;

        public StockApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> ProcessAsync(
            ProcessStockTransactionRequest request,
            string csrfToken)
        {
            if (string.IsNullOrWhiteSpace(csrfToken))
            {
                return "An antiforgery token is required. " +
                       "Refresh the page and try again.";
            }

            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                "api/stock/process");

            message.Headers.TryAddWithoutValidation(
                "X-CSRF-TOKEN",
                csrfToken);

            message.Content = JsonContent.Create(request);

            using var response = await _http.SendAsync(message);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
    }
}
