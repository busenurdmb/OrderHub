
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderHub.Infrastructure.Messaging;
using OrderHub.Infrastructure.Outbox.Helpers;
using OrderHub.Shared.Models;
using System.Text.Json;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(IServiceProvider serviceProvider, IEventPublisher eventPublisher, ILogger<OutboxPublisherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IOutboxDbContext>();

                var messages = dbContext.OutboxMessages
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(20)
                    .ToList();

                foreach (var message in messages)
                {
                    var type = TypeProvider.Get(message.Type); // ← düzeltildi
                    if (type == null) continue;

                    var @event = JsonSerializer.Deserialize(message.Content, type);
                    if (@event == null) continue;

                    await _eventPublisher.PublishAsync(@event, "order-created-exchange");

                    message.ProcessedOnUtc = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publishing failed");
            }

            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogInformation("Outbox publisher stopped");
    }
}
