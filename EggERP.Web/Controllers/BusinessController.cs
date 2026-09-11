using EggERP.Application.Businesses;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/business")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBusiness(Guid id)
    {
        var business = await _businessService.GetBusinessAsync(id);
        if (business is null)
        {
            return NotFound();
        }
        return Ok(business);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBusiness(Guid id, Business business)
    {
        if (id != business.Id)
        {
            return BadRequest("Route id does not match business id in the request body.");
        }
        var updated = await _businessService.UpdateBusinessAsync(business);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }
}