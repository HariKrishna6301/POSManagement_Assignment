using Dapper;
namespace POS.Data.Repositories;
public class WebhookRepository : IWebhookRepository
{
    private readonly SqlConnectionFactory _factory;
    public WebhookRepository(SqlConnectionFactory factory) => _factory = factory;
    public async Task LogAsync(string? orderNumber, string? status, string rawPayload, bool isValid)
    {
        using var db = _factory.Create();
        await db.ExecuteAsync("dbo.sp_WebhookLog_Insert",
            new { OrderNumber = orderNumber, Status = status, RawPayload = rawPayload, IsValid = isValid },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
