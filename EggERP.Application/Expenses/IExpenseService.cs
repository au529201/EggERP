using EggERP.Domain.Entities;
namespace EggERP.Application.Expenses;
public interface IExpenseService
{
    Task<List<Expense>> GetExpensesAsync(Guid businessId);
    Task<Expense?> GetExpenseByIdAsync(Guid businessId, Guid id);
    Task<Expense> CreateExpenseAsync(Expense expense);
}