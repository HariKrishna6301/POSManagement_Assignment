using POS.Business.DTOs;
namespace POS.Business.Services;
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<IEnumerable<ProductDto>> SearchAsync(string search);
    Task<int> CreateAsync(ProductDto dto);
    Task UpdateAsync(ProductDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ProductDto>> GetLowStockAsync();
}
