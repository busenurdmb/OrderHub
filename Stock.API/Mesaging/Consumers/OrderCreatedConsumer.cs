using System.Text;
using System.Text.Json;
using OrderHub.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Stock.API.Mesaging.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IStockService _stockService;
    private readonly IServiceScopeFactory _scopeFactory;
    public OrderCreatedConsumer(/*IStockService stockService, */IServiceScopeFactory scopeFactory, ILogger<OrderCreatedConsumer> logger)
    {
        //_stockService = stockService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5674,
            UserName = "admin",   // Veya guest
            Password = "password"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

      
        var exchangeName = "order-created-exchange";       // 📬 Exchange adı
        var queueName = "order-created-stock-queue";       // 📥 Stock servisine özel kuyruk

        //channel.QueueDeclare(queue: "order-created-queue", durable: true, exclusive: false, autoDelete: false);

        // 🎯 Fanout Exchange tanımı — mesajı tüm bağlı kuyruklara yollar
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);

        // 📥 Kuyruk oluşturuluyor
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        // 🧷 Kuyruk exchange'e bağlanıyor
        channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            //var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);
            var orderEvent = JsonSerializer.Deserialize<StockReservedEvent>(json);
            Console.WriteLine($"📦 Yeni Sipariş: {orderEvent?.CustomerName}");

            Console.WriteLine($"📦 [STOCK] Sipariş: {orderEvent?.CustomerName}");
            //if (orderEvent != null)
            //    await _stockService.HandleOrderCreatedAsync(orderEvent);
            if (orderEvent != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                await stockService.HandleOrderCreatedAsync(orderEvent);
            }

            _logger.LogInformation("📦 [STOCK] Sipariş işlendi: {Customer} - {TotalPrice} ₺", orderEvent.CustomerName, orderEvent.TotalPrice);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}
