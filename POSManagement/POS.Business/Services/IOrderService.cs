using POS.Business.DTOs;
using POS.Data.Models;
namespace POS.Business.Services;
public interface IOrderService
{
    Task<int> CreateAsync(OrderDto dto);
    Task<IEnumerable<Order>> HistoryAsync(DateTime? from, DateTime? to, string? status);
    Task<bool> ProcessPaymentAsync(PaymentWebhookDto dto);
    Task<DailySalesSummary> DailySummaryAsync(DateTime date);
}
