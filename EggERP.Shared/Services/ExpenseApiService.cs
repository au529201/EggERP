using System.Net.Http.Json;
using EggERP.Shared.Models;
namespace EggERP.Shared.Services
{
    public interface IExpenseApiService
    {
        Task<List<ExpenseDto>> GetExpensesAsync(Guid businessId);
        Task CreateExpenseAsync(CreateExpenseRequest request);
    }
    public class ExpenseApiService : IExpenseApiService
    {
        private readonly HttpClient _http;
        public ExpenseApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<ExpenseDto>> GetExpensesAsync(Guid businessId)
        {
            var result = await _http.GetFromJsonAsync<List<ExpenseDto>>(
                $"api/expenses/{businessId}");
            return result ?? new List<ExpenseDto>();
        }
        public async Task CreateExpenseAsync(CreateExpenseRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/expenses", request);
            response.EnsureSuccessStatusCode();
        }
    }
}