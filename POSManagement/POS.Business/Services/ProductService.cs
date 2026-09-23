using POS.Business.DTOs;
using POS.Data.Models;
using POS.Data.Repositories;
namespace POS.Business.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;
    private static ProductDto Map(Product p) => new() {
        ProductId=p.ProductId, Name=p.Name, SKU=p.SKU, Category=p.Category,
        Price=p.Price, StockQuantity=p.StockQuantity, LowStockThreshold=p.LowStockThreshold
    };
    public async Task<IEnumerable<ProductDto>> GetAllAsync() => (await _repo.GetAllAsync()).Select(Map);
    public async Task<IEnumerable<ProductDto>> SearchAsync(string search) => (await _repo.SearchAsync(search)).Select(Map);
    public async Task<int> CreateAsync(ProductDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Name) || string.IsNullOrWhiteSpace(d.SKU)) throw new ArgumentException("Name and SKU are required.");
        if (d.Price < 0 || d.StockQuantity < 0) throw new ArgumentException("Price and stock cannot be negative.");
        return await _repo.CreateAsync(new Product { Name=d.Name.Trim(), SKU=d.SKU.Trim(), Category=d.Category, Price=d.Price, StockQuantity=d.StockQuantity, LowStockThreshold=d.LowStockThreshold });
    }
    public async Task UpdateAsync(ProductDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Name) || string.IsNullOrWhiteSpace(d.SKU)) throw new ArgumentException("Name and SKU are required.");
        await _repo.UpdateAsync(new Product { ProductId=d.ProductId, Name=d.Name.Trim(), SKU=d.SKU.Trim(), Category=d.Category, Price=d.Price, StockQuantity=d.StockQuantity, LowStockThreshold=d.LowStockThreshold });
    }
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    public async Task<IEnumerable<ProductDto>> GetLowStockAsync() => (await _repo.GetLowStockAsync()).Select(Map);
}
