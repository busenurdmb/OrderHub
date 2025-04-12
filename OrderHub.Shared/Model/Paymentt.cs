namespace OrderHub.Shared.Models;

public class Paymentt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}

