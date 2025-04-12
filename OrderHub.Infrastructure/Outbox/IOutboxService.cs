namespace OrderHub.Infrastructure.Outbox;

public interface IOutboxService
{
    Task AddMessageAsync(object @event, string typeName);
}
