using POS.Business.DTOs;
using POS.Data.Repositories;
namespace POS.Business.Services;
public class WebhookService : IWebhookService
{
    private readonly IWebhookRepository _log;
    private readonly IOrderService _orders;
    private readonly IConfigurationAccessor _config;
    public WebhookService(IWebhookRepository log, IOrderService orders, IConfigurationAccessor config)
    { _log=log; _orders=orders; _config=config; }

    public async Task<bool> HandleAsync(PaymentWebhookDto dto, string rawPayload, string token)
    {
        var valid = !string.IsNullOrWhiteSpace(token) && token == _config.WebhookSecret;
        await _log.LogAsync(dto.OrderId, dto.Status, rawPayload, valid);
        if (!valid) return false;
        if (dto.Status is not ("success" or "failed")) return false;
        return await _orders.ProcessPaymentAsync(dto);
    }
}
public interface IConfigurationAccessor { string WebhookSecret { get; } }
public class ConfigurationAccessor : IConfigurationAccessor
{
    public string WebhookSecret { get; }
    public ConfigurationAccessor(string secret) => WebhookSecret = secret;
}
