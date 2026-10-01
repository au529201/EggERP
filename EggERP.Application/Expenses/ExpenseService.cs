using EggERP.Domain.Entities;

namespace EggERP.Application.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(
        IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public Task<List<Expense>> GetExpensesAsync(
        Guid businessId)
    {
        return _expenseRepository
            .GetByBusinessIdAsync(businessId);
    }

    public Task<Expense?> GetExpenseByIdAsync(
        Guid businessId,
        Guid id)
    {
        return _expenseRepository
            .GetByIdAsync(businessId, id);
    }

    public async Task<Expense> CreateExpenseAsync(
        Expense expense)
    {
        ValidateExpense(expense);

        PaymentValidation.EnsureValid(
            expense.PaymentMethod,
            expense.ReferenceNumber,
            expense.PaymentSource);

        expense.Id = Guid.NewGuid();
        expense.CreatedAtUtc = DateTime.UtcNow;

        expense.Description =
            expense.Description.Trim();

        expense.Category =
            string.IsNullOrWhiteSpace(expense.Category)
                ? string.Empty
                : expense.Category.Trim();

        expense.PaymentSource =
            string.IsNullOrWhiteSpace(expense.PaymentSource)
                ? null
                : expense.PaymentSource.Trim();

        expense.ReferenceNumber =
            string.IsNullOrWhiteSpace(expense.ReferenceNumber)
                ? null
                : expense.ReferenceNumber.Trim();

        if (expense.ExpenseDateUtc == default)
        {
            expense.ExpenseDateUtc = DateTime.UtcNow;
        }

        return await _expenseRepository
            .AddAsync(expense);
    }

    public async Task<bool> UpdateStatusAsync(
        Guid businessId,
        Guid id,
        string status)
    {
        ValidateStatus(status);

        var expense = await _expenseRepository
            .GetByIdAsync(businessId, id);

        if (expense is null)
        {
            return false;
        }

        if (expense.Status == status)
        {
            return true;
        }

        return await _expenseRepository
            .UpdateStatusAsync(
                businessId,
                id,
                status);
    }

    private static void ValidateExpense(
        Expense expense)
    {
        if (string.IsNullOrWhiteSpace(
            expense.Description))
        {
            throw new InvalidOperationException(
                "Expense description is required.");
        }

        if (expense.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Expense amount must be greater than zero.");
        }

        ValidateStatus(expense.Status);
    }

    private static void ValidateStatus(
        string status)
    {
        if (status != "Paid" &&
            status != "Pending")
        {
            throw new InvalidOperationException(
                "Expense status must be Paid or Pending.");
        }
    }
}