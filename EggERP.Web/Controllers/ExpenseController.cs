using EggERP.Application.ActivityLogs;
using EggERP.Application.Expenses;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize(Roles = "Admin,Manager")]
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
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var expenses = await _expenseService
            .GetExpensesAsync(currentUser.BusinessId);

        return Ok(expenses);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetExpenseById(
        Guid businessId,
        Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var expense = await _expenseService.GetExpenseByIdAsync(
            currentUser.BusinessId,
            id);

        if (expense is null)
        {
            return NotFound();
        }

        return Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense(Expense expense)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        // Never trust BusinessId supplied by the client.
        expense.BusinessId = currentUser.BusinessId;

        try
        {
            var created =
                await _expenseService.CreateExpenseAsync(expense);

            await _activityLogService.LogAsync(
                currentUser.BusinessId,
                currentUser.Id,
                currentUser.FullName,
                "Created Expense",
                "Expense",
                created.Id,
                $"{created.Description} — " +
                $"{created.Amount:N2} " +
                $"({created.PaymentMethod})");

            return CreatedAtAction(
                nameof(GetExpenses),
                new { businessId = currentUser.BusinessId },
                created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    public class UpdateExpenseStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
    Guid id,
    UpdateExpenseStatusRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (request.Status != "Paid" &&
            request.Status != "Pending")
        {
            return BadRequest(
                "Expense status must be Paid or Pending.");
        }

        var expense = await _expenseService.GetExpenseByIdAsync(
            currentUser.BusinessId,
            id);

        if (expense is null)
        {
            return NotFound();
        }

        var oldStatus = expense.Status;

        if (oldStatus == request.Status)
        {
            return NoContent();
        }

        try
        {
            var updated = await _expenseService.UpdateStatusAsync(
                currentUser.BusinessId,
                id,
                request.Status);

            if (!updated)
            {
                return NotFound();
            }

            await _activityLogService.LogAsync(
                currentUser.BusinessId,
                currentUser.Id,
                currentUser.FullName,
                "Updated Expense Status",
                "Expense",
                id,
                $"{expense.Description} — " +
                $"{oldStatus} → {request.Status} — " +
                $"{expense.Amount:N2}");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}