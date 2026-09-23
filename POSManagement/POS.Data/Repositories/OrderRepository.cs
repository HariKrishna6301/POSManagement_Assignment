using System.Data;
using Dapper;
using POS.Data.Models;
namespace POS.Data.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly SqlConnectionFactory _factory;
    public OrderRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<int> CreateOrderAsync(Order order)
    {
        using var db = _factory.Create();

        var dt = new DataTable();

        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("ProductName", typeof(string));
        dt.Columns.Add("Quantity", typeof(int));
        dt.Columns.Add("UnitPrice", typeof(decimal));
        dt.Columns.Add("LineTotal", typeof(decimal));

        foreach (var item in order.Items)
        {
            dt.Rows.Add(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal
            );
        }

        var parameters = new DynamicParameters();

        parameters.Add("@OrderNumber", order.OrderNumber);
        parameters.Add("@Subtotal", order.Subtotal);
        parameters.Add("@Discount", order.Discount);
        parameters.Add("@Tax", order.Tax);
        parameters.Add("@GrandTotal", order.GrandTotal);

        parameters.Add(
            "@Items",
            dt.AsTableValuedParameter("dbo.OrderItemType")
        );

        return await db.ExecuteScalarAsync<int>(
            "dbo.sp_CreateOrder",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Order>> GetHistoryAsync(DateTime? from, DateTime? to, string? status)
    {
        using var db = _factory.Create();
        var orders = (await db.QueryAsync<Order>("dbo.sp_GetOrderHistory",
            new { FromDate = from, ToDate = to, PaymentStatus = status },
            commandType: CommandType.StoredProcedure)).ToList();

        foreach (var order in orders)
            order.Items = (await db.QueryAsync<OrderItem>("dbo.sp_GetOrderItems",
                new { OrderId = order.OrderId }, commandType: CommandType.StoredProcedure)).ToList();

        return orders;
    }

    public async Task<Order?> GetByNumberAsync(string orderNumber)
    {
        using var db = _factory.Create();
        return await db.QueryFirstOrDefaultAsync<Order>("dbo.sp_GetOrderByNumber",
            new { OrderNumber = orderNumber }, commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ProcessPaymentAsync(string orderNumber, string status, decimal amount, string? transactionId)
    {
        using var db = _factory.Create();
        return await db.ExecuteScalarAsync<bool>("dbo.sp_ProcessPayment",
            new { OrderNumber = orderNumber, Status = status, Amount = amount, TransactionId = transactionId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DailySalesSummary> GetDailySummaryAsync(DateTime date)
    {
        using var db = _factory.Create();
        return await db.QuerySingleAsync<DailySalesSummary>("dbo.sp_GetDailySalesSummary",
            new { SaleDate = date.Date }, commandType: CommandType.StoredProcedure);
    }
}
