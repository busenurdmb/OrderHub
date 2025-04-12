namespace OrderHub.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string queueName);
}
