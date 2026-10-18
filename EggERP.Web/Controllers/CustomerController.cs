
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
[Authorize(Policy = "ActiveBusiness")]
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

        return user is not null &&
               user.IsActive &&
               user.BusinessId != Guid.Empty
            ? user
            : null;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetCustomers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        return Ok(await _customerService.GetCustomersAsync(businessId));
    }

    [HttpGet("{businessId:guid}/all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllCustomers(Guid businessId)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        return Ok(await _customerService.GetAllCustomersAsync(businessId));
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid businessId, Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        var customer = await _customerService
            .GetCustomerByIdAsync(businessId, id);

        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] Customer customer)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (customer is null ||
            string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest("Customer name is required.");

        customer.Id = Guid.NewGuid();
        customer.BusinessId = user.BusinessId;
        customer.Name = customer.Name.Trim();

        var created = await _customerService.CreateCustomerAsync(customer);

        await _activityLogService.LogAsync(
            user.BusinessId,
            user.Id,
            user.FullName,
            "Created Customer",
            "Customer",
            created.Id,
            created.Name);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { businessId = user.BusinessId, id = created.Id },
            created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] Customer customer)
    {
        var user = await GetActiveUserAsync();

        if (user is null)
            return Forbid();

        if (customer is null || id != customer.Id)
            return BadRequest("Customer ID mismatch.");

        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest("Customer name is required.");

        var existing = await _customerService
            .GetCustomerByIdAsync(user.BusinessId, id);

        if (existing is null)
            return NotFound();

        customer.BusinessId = user.BusinessId;
        customer.Name = customer.Name.Trim();

        if (!await _customerService.UpdateCustomerAsync(customer))
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateCustomer(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        if (!await _customerService.DeactivateCustomerAsync(businessId, id))
            return NotFound();

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Deactivated Customer",
            "Customer",
            id,
            null);

        return NoContent();
    }

    [HttpPatch("{businessId:guid}/{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ReactivateCustomer(
        Guid businessId,
        Guid id)
    {
        var user = await GetActiveUserAsync();

        if (user is null || user.BusinessId != businessId)
            return Forbid();

        if (!await _customerService.ReactivateCustomerAsync(businessId, id))
            return NotFound();

        await _activityLogService.LogAsync(
            businessId,
            user.Id,
            user.FullName,
            "Reactivated Customer",
            "Customer",
            id,
            null);

        return NoContent();
    }
}
