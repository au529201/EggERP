using EggERP.Domain.Entities;

namespace EggERP.Application.Expenses;

public interface IExpenseRepository
{
    Task<List<Expense>> GetByBusinessIdAsync(Guid businessId);
    Task<Expense?> GetByIdAsync(Guid businessId, Guid id);
    Task<Expense> AddAsync(Expense expense);

    Task<bool> UpdateStatusAsync(
        Guid businessId,
        Guid id,
        string status);
}