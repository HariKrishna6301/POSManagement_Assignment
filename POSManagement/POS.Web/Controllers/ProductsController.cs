using Microsoft.AspNetCore.Mvc;
using POS.Business.DTOs;
using POS.Business.Services;
namespace POS.Web.Controllers;
public class ProductsController : Controller
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service=service;
    public async Task<IActionResult> Index(string? search) => View(await (string.IsNullOrWhiteSpace(search) ? _service.GetAllAsync() : _service.SearchAsync(search)));
    [HttpGet] public IActionResult Create() => View(new ProductDto { LowStockThreshold=5 });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try { await _service.CreateAsync(dto); TempData["Success"]="Product created."; return RedirectToAction(nameof(Index)); }
        catch(Exception ex) { ModelState.AddModelError("", ex.Message); return View(dto); }
    }
    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        var p=(await _service.GetAllAsync()).FirstOrDefault(x=>x.ProductId==id);
        return p is null ? NotFound() : View(p);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try { await _service.UpdateAsync(dto); TempData["Success"]="Product updated."; return RedirectToAction(nameof(Index)); }
        catch(Exception ex) { ModelState.AddModelError("", ex.Message); return View(dto); }
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return RedirectToAction(nameof(Index)); }
}
