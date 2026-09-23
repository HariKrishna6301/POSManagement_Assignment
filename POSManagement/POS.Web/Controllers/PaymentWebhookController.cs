using Microsoft.AspNetCore.Mvc;
using POS.Business.DTOs;
using POS.Business.Services;
using System.Text.Json;
namespace POS.Web.Controllers;
[ApiController]
[Route("api/payment")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IWebhookService _service;
    public PaymentWebhookController(IWebhookService service) => _service=service;

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookDto dto)
    {
        var raw = JsonSerializer.Serialize(dto);
        var token = Request.Headers["X-Webhook-Token"].FirstOrDefault() ?? "";
        var ok = await _service.HandleAsync(dto, raw, token);
        return ok ? Ok(new { success=true, message="Webhook processed." }) : Unauthorized(new { success=false, message="Invalid webhook or processing failed." });
    }
}
