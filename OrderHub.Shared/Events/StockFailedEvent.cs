namespace OrderHub.Shared.Events;

public class StockFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}
