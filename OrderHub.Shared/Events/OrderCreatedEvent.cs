
namespace OrderHub.Shared.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItemMessage> Items { get; set; } = new();
}

public class OrderItemMessage
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
