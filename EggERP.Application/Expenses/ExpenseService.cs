using EggERP.Domain.Entities;
namespace EggERP.Application.Expenses;
public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }
    public Task<List<Expense>> GetExpensesAsync(Guid businessId)
    {
        return _expenseRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Expense?> GetExpenseByIdAsync(Guid businessId, Guid id)
    {
        return _expenseRepository.GetByIdAsync(businessId, id);
    }
    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        PaymentValidation.EnsureValid(expense.PaymentMethod, expense.ReferenceNumber, expense.PaymentSource);
        expense.Id = Guid.NewGuid();
        expense.CreatedAtUtc = DateTime.UtcNow;
        if (expense.ExpenseDateUtc == default)
        {
            expense.ExpenseDateUtc = DateTime.UtcNow;
        }
        return await _expenseRepository.AddAsync(expense);
    }
}