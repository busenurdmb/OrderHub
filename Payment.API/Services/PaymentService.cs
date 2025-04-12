using OrderHub.Shared.Models;
using System.Data;
using Dapper;
using Npgsql;
using OrderHub.Shared.Events;
using Payment.API.Services;
using Microsoft.Extensions.Configuration;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly string _connectionString;

    public PaymentService(ILogger<PaymentService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Postgres"); ;
    }

    public async Task ProcessPaymentAsync(StockReservedEvent orderEvent)
    {

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var payment = new Paymentt
        {
            OrderId = orderEvent.OrderId,
            CustomerName = orderEvent.CustomerName,
            TotalAmount = orderEvent.TotalPrice
        };

        const string sql = @"INSERT INTO ""Payments"" (""Id"", ""OrderId"", ""CustomerName"", ""TotalAmount"", ""PaidAt"")
                     VALUES (@Id, @OrderId, @CustomerName, @TotalAmount, @PaidAt);";



        await connection.ExecuteAsync(sql, payment);

        _logger.LogInformation("💸 Ödeme tamamlandı ve veritabanına kaydedildi: {OrderId}", payment.OrderId);
    }
}

