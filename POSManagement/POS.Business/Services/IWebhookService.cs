using POS.Business.DTOs;
namespace POS.Business.Services;
public interface IWebhookService
{
    Task<bool> HandleAsync(PaymentWebhookDto dto, string rawPayload, string token);
}
