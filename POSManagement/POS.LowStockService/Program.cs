using Dapper;
using POS.Data;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(new SqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddHostedService<LowStockWorker>();
await builder.Build().RunAsync();

public class LowStockWorker : BackgroundService
{
    private readonly SqlConnectionFactory _factory;
    public LowStockWorker(SqlConnectionFactory factory) => _factory=factory;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var db=_factory.Create();
                var rows=await db.QueryAsync<dynamic>("dbo.sp_GetLowStockProducts",commandType:System.Data.CommandType.StoredProcedure);
                var folder=Path.Combine(AppContext.BaseDirectory,"Reports");
                Directory.CreateDirectory(folder);
                var file=Path.Combine(folder,$"low-stock-{DateTime.Now:yyyyMMdd}.csv");
                await File.WriteAllLinesAsync(file, new[]{"SKU,Name,Stock,Threshold"}.Concat(rows.Select(x=>$"{x.SKU},{x.Name},{x.StockQuantity},{x.LowStockThreshold}")), stoppingToken);
            } catch { }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
