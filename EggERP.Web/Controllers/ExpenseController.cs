using EggERP.Application.Expenses;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/expenses")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetExpenses(Guid businessId)
    {
        var expenses = await _expenseService.GetExpensesAsync(businessId);
        return Ok(expenses);
    }
    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetExpenseById(Guid businessId, Guid id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(businessId, id);
        if (expense is null)
        {
            return NotFound();
        }
        return Ok(expense);
    }
    [HttpPost]
    public async Task<IActionResult> CreateExpense(Expense expense)
    {
        try
        {
            var created = await _expenseService.CreateExpenseAsync(expense);
            return CreatedAtAction(nameof(GetExpenses), new { businessId = created.BusinessId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}