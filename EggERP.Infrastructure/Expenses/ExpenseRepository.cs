using EggERP.Application.Expenses;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Expenses;
public class ExpenseRepository : IExpenseRepository
{
    private readonly EggERPDbContext _dbContext;
    public ExpenseRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<Expense>> GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Expenses
            .Where(e => e.BusinessId == businessId)
            .OrderByDescending(e => e.ExpenseDateUtc)
            .ToListAsync();
    }
    public Task<Expense?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.BusinessId == businessId && e.Id == id);
    }
    public async Task<Expense> AddAsync(Expense expense)
    {
        _dbContext.Expenses.Add(expense);

        await _dbContext.SaveChangesAsync();

        return expense;
    }

    public async Task<bool> UpdateStatusAsync(
        Guid businessId,
        Guid id,
        string status)
    {
        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e =>
                e.BusinessId == businessId &&
                e.Id == id);

        if (expense is null)
        {
            return false;
        }

        expense.Status = status;
        expense.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }
}