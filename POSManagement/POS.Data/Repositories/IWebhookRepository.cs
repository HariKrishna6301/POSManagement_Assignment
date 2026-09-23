namespace POS.Data.Repositories;
public interface IWebhookRepository
{
    Task LogAsync(string? orderNumber, string? status, string rawPayload, bool isValid);
}
