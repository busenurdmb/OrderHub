using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OrderHub.Persistence.DbContexts;
using OrderHub.Shared.Models;

namespace OrderHub.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    //private readonly string _connectionString;

    //public OrderRepository(IConfiguration configuration)
    //{
    //    _connectionString = configuration.GetConnectionString("Postgres")!;
    //}
    private readonly DapperConnectionProvider _provider;

    public OrderRepository(DapperConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task<Guid> CreateOrderAsync(Orderr order)
    {
        const string sql = @"INSERT INTO ""Orders"" (""Id"", ""CustomerName"", ""TotalPrice"", ""CreatedAt"")
                     VALUES (@Id, @CustomerName, @TotalPrice, @CreatedAt);";
        var parameters = new DynamicParameters();
        parameters.Add("Id", order.Id);
        parameters.Add("CustomerName", order.CustomerName);
        parameters.Add("TotalPrice", order.TotalPrice);
        parameters.Add("CreatedAt", order.CreatedAt);


        const string itemSql = @"INSERT INTO ""OrderItems"" (""Id"", ""OrderId"", ""ProductId"", ""Quantity"")
                             VALUES (@Id, @OrderId, @ProductId, @Quantity);";

        using var connection = _provider.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);

        // 🧩 Siparişe ait ürünleri de ekle
        foreach (var item in order.Items)
        {
            item.Id = Guid.NewGuid();
            item.OrderId = order.Id;

            await connection.ExecuteAsync(itemSql, new
            {
                item.Id,
                item.OrderId,
                item.ProductId,
                item.Quantity
            });
        }

        //await connection.ExecuteAsync(sql, order);
        return order.Id;
    }
    public async Task<IEnumerable<Orderr>> GetAllOrdersAsync()
    {
        const string sql = @"SELECT * FROM ""Orders""";
        using var connection = _provider.CreateConnection();
        var orders = await connection.QueryAsync<Orderr>(sql);
        return orders;
    }

    public async Task<bool> UpdateOrderAsync(Orderr order)
    {
        const string sql = @"UPDATE ""Orders"" SET 
                         ""CustomerName"" = @CustomerName,
                         ""TotalPrice"" = @TotalPrice
                         WHERE ""Id"" = @Id";

        using var connection = _provider.CreateConnection();
        var result = await connection.ExecuteAsync(sql, order);
        return result > 0;
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        const string sql = @"DELETE FROM ""Orders"" WHERE ""Id"" = @Id";
        using var connection = _provider.CreateConnection();
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<Orderr?> GetOrderByIdAsync(Guid orderId)
    {
        const string sql = @"Selet * FROM ""Orders"" WHERE ""Id"" = @Id";
        using var connection = _provider.CreateConnection();
        var order = await connection.QueryFirstOrDefaultAsync<Orderr>(sql, new { Id = orderId });
        return order;
    }
}
