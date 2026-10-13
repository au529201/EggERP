
using EggERP.Application.ActivityLogs;
using EggERP.Application.Customers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,Staff")]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public CustomerController(
        ICustomerService customerService,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _customerService = customerService;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    private async Task<ApplicationUser?> GetActiveUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null ||
            !user.IsActive ||
            user.BusinessId == Guid.Empty)
        {
            return null;
        }

        return user;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetCustomers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var customers =
            await _customerService.GetCustomersAsync(user.BusinessId);

        return Ok(customers);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetCustomerById(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var customer =
            await _customerService.GetCustomerByIdAsync(
                user.BusinessId,
                id);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] Customer customer)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest("Customer name is required.");

        customer.Id = Guid.NewGuid();
        customer.BusinessId = user.BusinessId;
        customer.Name = customer.Name.Trim();

        var createdCustomer =
            await _customerService.CreateCustomerAsync(customer);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Customer",
            "Customer",
            createdCustomer.Id,
            createdCustomer.Name);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new
            {
                businessId = user.BusinessId,
                id = createdCustomer.Id
            },
            createdCustomer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] Customer customer)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (id != customer.Id)
            return BadRequest("Customer ID mismatch.");

        var existing =
            await _customerService.GetCustomerByIdAsync(
                user.BusinessId,
                id);

        if (existing is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest("Customer name is required.");

        customer.BusinessId = user.BusinessId;
        customer.Name = customer.Name.Trim();

        var updated =
            await _customerService.UpdateCustomerAsync(customer);

        if (!updated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Updated Customer",
            "Customer",
            id,
            customer.Name);

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateCustomer(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var deactivated =
            await _customerService.DeactivateCustomerAsync(
                user.BusinessId,
                id);

        if (!deactivated)
            return NotFound();

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Deactivated Customer",
            "Customer",
            id,
            null);

        return NoContent();
    }
}
