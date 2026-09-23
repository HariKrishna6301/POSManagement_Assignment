using Microsoft.AspNetCore.Mvc;
using POS.Business.Services;
namespace POS.Web.Controllers;
public class OrdersController : Controller
{
    private readonly IOrderService _service;
    public OrdersController(IOrderService service) => _service=service;
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? status)
        => View(await _service.HistoryAsync(from, to, string.IsNullOrWhiteSpace(status) ? null : status));
    public async Task<IActionResult> DailyReport(DateTime? date)
        => View(await _service.DailySummaryAsync(date?.Date ?? DateTime.Today));
}
