using EggERP.Application.Customers;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
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
        return NoContent();
    }
}