using EggERP.Application.Payments;
using EggERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EggERP.Web.Controllers;
[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetPayments(Guid businessId)
    {
        var payments = await _paymentService.GetPaymentsAsync(businessId);
        return Ok(payments);
    }
    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetPaymentById(Guid businessId, Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(businessId, id);
        if (payment is null)
        {
            return NotFound();
        }
        return Ok(payment);
    }
    [HttpPost]
    public async Task<IActionResult> CreatePayment(Payment payment)
    {
        var created = await _paymentService.CreatePaymentAsync(payment);
        return CreatedAtAction(nameof(GetPayments), new { businessId = created.BusinessId }, created);
    }
}