using OrderHub.Shared.DTOs;
using OrderHub.Shared.Model;

namespace OrderHub.Shared.Models;

public class Orderr
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}
