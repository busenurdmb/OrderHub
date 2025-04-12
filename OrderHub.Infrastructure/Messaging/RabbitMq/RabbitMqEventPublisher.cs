using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using RabbitMQ.Client;

namespace OrderHub.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMQ.Client.IConnection _connection;


    public RabbitMqEventPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5674, // docker-compose'da bu port tanımlı
            UserName = "admin",
            Password = "password"
        };

        _connection = factory.CreateConnection();
    }

    public Task PublishAsync<T>(T @event, string exchangeName)
    {
        using var channel = _connection.CreateModel();
        //channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        // FANOUT exchange kullandığımız için exchange declare edilir
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        // RoutingKey boş çünkü fanout exchange tüm kuyruklara gönderir
        channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, body: body);
        //channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        return Task.CompletedTask;
    }
}
