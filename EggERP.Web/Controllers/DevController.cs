using EggERP.Application.Email;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _env;

    public DevController(IEmailSender emailSender, IWebHostEnvironment env)
    {
        _emailSender = emailSender;
        _env = env;
    }

    [HttpGet("test-email")]
    public async Task<IActionResult> TestEmail([FromQuery] string to)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        await _emailSender.SendEmailAsync(to, "EggERP test email", "<p>This is a test email from EggERP via Brevo.</p>");
        return Ok(new { message = $"Test email sent to {to}" });
    }
}