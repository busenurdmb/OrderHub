using Dapper;
using Npgsql;
using OrderHub.Infrastructure.Messaging;
using OrderHub.Shared.Events;

public class StockService : IStockService
{
    private readonly string _connectionString;
    private readonly IEventPublisher _eventPublisher;
    public StockService(IConfiguration configuration, IEventPublisher eventPublisher)
    {
        _connectionString = configuration.GetConnectionString("Postgres");
        _eventPublisher = eventPublisher;
    }
    public async Task HandleOrderCreatedAsync(StockReservedEvent orderEvent)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        var allAvailable = true;

        // 1. Önce stok yeterli mi kontrol et
        foreach (var item in orderEvent.Items)
        {
            var stockCheckSql = @"SELECT ""Stock"" FROM ""Products"" WHERE ""Id"" = @ProductId";
            var stock = await connection.ExecuteScalarAsync<int>(stockCheckSql, new { item.ProductId });

            if (stock < item.Quantity)
            {
                allAvailable = false;
                break;
            }
        }

        if (!allAvailable)
        {
            Console.WriteLine("❌ Stok yetersiz, sipariş reddedildi.");

            var failedEvent = new StockFailedEvent
            {
                OrderId = orderEvent.OrderId,
                CustomerName = orderEvent.CustomerName,
                Reason = "Yetersiz stok"
            };

            await _eventPublisher.PublishAsync(failedEvent, "stock-failed-exchange");
            return;
        }

        // 2. Stoklar uygunsa, güncelle
        foreach (var item in orderEvent.Items)
        {
            var updateSql = @"UPDATE ""Products"" SET ""Stock"" = ""Stock"" - @Quantity WHERE ""Id"" = @ProductId";
            await connection.ExecuteAsync(updateSql, new { item.ProductId, item.Quantity }, transaction);
        }

        await transaction.CommitAsync();

        Console.WriteLine("✅ Stok güncellendi, ödeme süreci başlatılıyor...");

        var reservedEvent = new StockReservedEvent
        {
            OrderId = orderEvent.OrderId,
            CustomerName = orderEvent.CustomerName,
            TotalPrice = orderEvent.TotalPrice,
            CreatedAt = orderEvent.CreatedAt,
            Items = orderEvent.Items
        };

        await _eventPublisher.PublishAsync(reservedEvent, "stock-reserved-exchange");
    }

    //public async Task HandleOrderCreatedAsync(StockReservedEvent orderEvent)
    //{
    //    await using var connection = new NpgsqlConnection(_connectionString);
    //    await connection.OpenAsync();

    //    using var transaction = await connection.BeginTransactionAsync();

    //    var allAvailable = true;

    //    foreach (var item in orderEvent.Items)
    //    {
    //        var sql = @"UPDATE ""Products""
    //                    SET ""Stock"" = ""Stock"" - @Quantity
    //                    WHERE ""Id"" = @ProductId AND ""Stock"" >= @Quantity";

    //        var result = await connection.ExecuteAsync(sql, new
    //        {
    //            ProductId = item.ProductId,
    //            Quantity = item.Quantity
    //        });

    //        if (result > 0)
    //            Console.WriteLine($"✅ [{item.ProductId}] stoğu {item.Quantity} azaltıldı.");
    //        else
    //            Console.WriteLine($"❗ Yetersiz stok: {item.ProductId} - {item.Quantity}");
    //    }
    //}
}

