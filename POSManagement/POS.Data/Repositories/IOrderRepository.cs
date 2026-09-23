using POS.Data.Models;
namespace POS.Data.Repositories;
public interface IOrderRepository
{
    Task<int> CreateOrderAsync(Order order);
    Task<IEnumerable<Order>> GetHistoryAsync(DateTime? from, DateTime? to, string? status);
    Task<Order?> GetByNumberAsync(string orderNumber);
    Task<bool> ProcessPaymentAsync(string orderNumber, string status, decimal amount, string? transactionId);
    Task<DailySalesSummary> GetDailySummaryAsync(DateTime date);
}
