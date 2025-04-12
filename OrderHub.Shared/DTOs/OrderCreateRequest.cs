

namespace OrderHub.Shared.DTOs;

public class OrderCreateRequest
{
    public string CustomerName { get; set; } = null!;
    public decimal TotalPrice { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}