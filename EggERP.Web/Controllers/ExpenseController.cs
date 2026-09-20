using EggERP.Application.ActivityLogs;
using EggERP.Application.Expenses;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public ExpenseController(
        IExpenseService expenseService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _expenseService = expenseService;
        _userManager = userManager;
        _activityLogService = activityLogService;
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is not null)
            {
                await _activityLogService.LogAsync(
                    created.BusinessId, currentUser.Id, currentUser.FullName,
                    "Created Expense", "Expense", created.Id,
                    $"{created.Description} — {created.Amount:N2} ({created.PaymentMethod})");
            }

            return CreatedAtAction(nameof(GetExpenses), new { businessId = created.BusinessId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}