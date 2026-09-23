using Microsoft.AspNetCore.Mvc;
using POS.Business.DTOs;
using POS.Business.Services;
namespace POS.Web.Controllers;
public class POSController : Controller
{
    private readonly IProductService _products;
    private readonly IOrderService _orders;
    public POSController(IProductService products, IOrderService orders) { _products=products; _orders=orders; }
    public async Task<IActionResult> Index(string? search) => View(await (string.IsNullOrWhiteSpace(search) ? _products.GetAllAsync() : _products.SearchAsync(search)));
    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] OrderDto dto)
    {
        try { var id=await _orders.CreateAsync(dto); return Json(new { success=true, orderId=id, orderNumber=dto.OrderNumber }); }
        catch(Exception ex) { return BadRequest(new { success=false, message=ex.Message }); }
    }
}
