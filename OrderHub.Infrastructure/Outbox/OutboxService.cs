using System.Text.Json;
using OrderHub.Shared.Models;

namespace OrderHub.Infrastructure.Outbox;

// OrderHub.Infrastructure/Outbox/OutboxService.cs
public class OutboxService : IOutboxService
{
    private readonly IOutboxDbContext _dbContext;

    public OutboxService(IOutboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddMessageAsync(object @event, string typeName)
    {
        var message = new OutboxMessage
        {
            Type = typeName,
            Content = JsonSerializer.Serialize(@event)
        };

        await _dbContext.OutboxMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
    }
}

