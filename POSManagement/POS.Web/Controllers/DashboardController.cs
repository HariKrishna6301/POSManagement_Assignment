using Microsoft.AspNetCore.Mvc;
using POS.Business.Services;
namespace POS.Web.Controllers;
public class DashboardController : Controller
{
    private readonly IProductService _products;
    private readonly IOrderService _orders;
    public DashboardController(IProductService products, IOrderService orders) { _products=products; _orders=orders; }
    public async Task<IActionResult> Index()
    {
        ViewBag.LowStock = (await _products.GetLowStockAsync()).Count();
        ViewBag.Summary = await _orders.DailySummaryAsync(DateTime.Today);
        return View();
    }
}
