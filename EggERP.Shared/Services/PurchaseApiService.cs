using System.Net.Http.Json;
using EggERP.Shared.Models;

namespace EggERP.Shared.Services
{
    public interface IPurchaseApiService
    {
        Task<List<PurchaseDto>> GetPurchasesAsync(Guid businessId);
        Task CreatePurchaseAsync(CreatePurchaseRequest request);

        Task MarkAsPaidAsync(
            Guid businessId,
            Guid purchaseId,
            string paymentMethod,
            string? paymentSource,
            string? referenceNumber,
            string? bankName);
    }

    public class PurchaseApiService : IPurchaseApiService
    {
        private readonly HttpClient _http;

        public PurchaseApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PurchaseDto>> GetPurchasesAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<PurchaseDto>>(
                $"api/purchases/{businessId}");

            return result ?? new List<PurchaseDto>();
        }

        public async Task CreatePurchaseAsync(CreatePurchaseRequest request)
        {
            var response = await _http.PostAsJsonAsync(
                "api/purchases",
                request);

            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAsPaidAsync(
            Guid businessId,
            Guid purchaseId,
            string paymentMethod,
            string? paymentSource,
            string? referenceNumber,
            string? bankName)
        {
            var request = new
            {
                PaymentMethod = paymentMethod,
                PaymentSource = paymentSource,
                ReferenceNumber = referenceNumber,
                BankName = bankName
            };

            var response = await _http.PutAsJsonAsync(
                $"api/purchases/{businessId}/{purchaseId}/mark-paid",
                request);

            response.EnsureSuccessStatusCode();
        }
    }
}