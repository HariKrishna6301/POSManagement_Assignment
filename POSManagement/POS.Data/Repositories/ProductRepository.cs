using Dapper;
using POS.Data.Models;
namespace POS.Data.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly SqlConnectionFactory _factory;
    public ProductRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var db = _factory.Create();
        return await db.QueryAsync<Product>("dbo.sp_Product_GetAll", commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task<IEnumerable<Product>> SearchAsync(string search)
    {
        using var db = _factory.Create();
        return await db.QueryAsync<Product>("dbo.sp_Product_Search", new { Search = search ?? "" }, commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task<int> CreateAsync(Product p)
    {
        using var db = _factory.Create();
        return await db.ExecuteScalarAsync<int>("dbo.sp_Product_Insert",
            new { p.Name, p.SKU, p.Category, p.Price, p.StockQuantity, p.LowStockThreshold },
            commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task UpdateAsync(Product p)
    {
        using var db = _factory.Create();
        await db.ExecuteAsync("dbo.sp_Product_Update",
            new { p.ProductId, p.Name, p.SKU, p.Category, p.Price, p.StockQuantity, p.LowStockThreshold },
            commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task DeleteAsync(int id)
    {
        using var db = _factory.Create();
        await db.ExecuteAsync("dbo.sp_Product_Delete", new { ProductId = id }, commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task<IEnumerable<Product>> GetLowStockAsync()
    {
        using var db = _factory.Create();
        return await db.QueryAsync<Product>("dbo.sp_GetLowStockProducts", commandType: System.Data.CommandType.StoredProcedure);
    }
}
