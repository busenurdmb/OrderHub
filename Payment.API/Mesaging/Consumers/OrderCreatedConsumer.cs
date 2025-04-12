using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderHub.Shared.Events;
using Payment.API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using StackExchange.Redis;


namespace Payment.API.Messaging.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5674,
            UserName = "admin",
            Password = "password"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
   
        //var exchangeName = "order-created-exchange";
        var exchangeName = "stock-reserved-exchange";
        var queueName = "stock-reserved-payment-queue";

        //var queueName = "order-created-payment-queue";

        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var orderEvent = JsonSerializer.Deserialize<StockReservedEvent>(json);

            Console.WriteLine($"📦 Yeni Sipariş: {orderEvent?.CustomerName}");

            Console.WriteLine($"📦 [STOCK] Sipariş: {orderEvent?.CustomerName}");
        
            if (orderEvent != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                await paymentService.ProcessPaymentAsync(orderEvent);
            }
            _logger.LogInformation("💳 [PAYMENT] Ödeme işlendi: {Customer} - {Amount} ₺", orderEvent.CustomerName, orderEvent.TotalPrice);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
