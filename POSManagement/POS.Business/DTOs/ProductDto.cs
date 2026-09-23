namespace POS.Business.DTOs;
public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
}
