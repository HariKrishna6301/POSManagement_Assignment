using POS.Business.DTOs;
using POS.Data.Models;
using POS.Data.Repositories;
namespace POS.Business.Services;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;

    public Task<int> CreateAsync(OrderDto dto)
    {
        if (dto.Items.Count == 0) throw new ArgumentException("Cart is empty.");
        if (dto.Discount < 0 || dto.Tax < 0) throw new ArgumentException("Discount/tax cannot be negative.");
        var order = new Order {
            OrderNumber = string.IsNullOrWhiteSpace(dto.OrderNumber) ? $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}" : dto.OrderNumber,
            Subtotal=dto.Subtotal, Discount=dto.Discount, Tax=dto.Tax, GrandTotal=dto.GrandTotal,
            Items=dto.Items.Select(i => new OrderItem { ProductId=i.ProductId, ProductName=i.ProductName, Quantity=i.Quantity, UnitPrice=i.UnitPrice, LineTotal=i.LineTotal }).ToList()
        };
        return _repo.CreateOrderAsync(order);
    }
    public Task<IEnumerable<Order>> HistoryAsync(DateTime? from, DateTime? to, string? status) => _repo.GetHistoryAsync(from, to, status);
    public Task<bool> ProcessPaymentAsync(PaymentWebhookDto dto) => _repo.ProcessPaymentAsync(dto.OrderId, dto.Status, dto.Amount, dto.TransactionId);
    public Task<DailySalesSummary> DailySummaryAsync(DateTime date) => _repo.GetDailySummaryAsync(date);
}
