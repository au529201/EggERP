using EggERP.Application.ActivityLogs;
using EggERP.Application.Customers;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
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

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetCustomers(Guid businessId)
    {
        var customers = await _customerService.GetCustomersAsync(businessId);
        return Ok(customers);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid businessId, Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(businessId, id);
        if (customer is null)
        {
            return NotFound();
        }
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(Customer customer)
    {
        var createdCustomer = await _customerService.CreateCustomerAsync(customer);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                createdCustomer.BusinessId, currentUser.Id, currentUser.FullName,
                "Created Customer", "Customer", createdCustomer.Id, createdCustomer.Name);
        }

        return CreatedAtAction(
            nameof(GetCustomers),
            new { businessId = createdCustomer.BusinessId },
            createdCustomer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest("Route id does not match customer id in the request body.");
        }
        var updated = await _customerService.UpdateCustomerAsync(customer);
        if (!updated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                customer.BusinessId, currentUser.Id, currentUser.FullName,
                "Updated Customer", "Customer", id, customer.Name);
        }

        return NoContent();
    }

    [HttpDelete("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> DeactivateCustomer(Guid businessId, Guid id)
    {
        var deactivated = await _customerService.DeactivateCustomerAsync(businessId, id);
        if (!deactivated)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null)
        {
            await _activityLogService.LogAsync(
                businessId, currentUser.Id, currentUser.FullName,
                "Deactivated Customer", "Customer", id, null);
        }

        return NoContent();
    }
} 