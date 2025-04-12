namespace OrderHub.Shared.Models;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedOnUtc { get; set; }
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
}
