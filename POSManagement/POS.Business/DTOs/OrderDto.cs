namespace POS.Business.DTOs;
public class OrderDto
{
    public string OrderNumber { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}
public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
public class PaymentWebhookDto
{
    public string OrderId { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = "";
}
