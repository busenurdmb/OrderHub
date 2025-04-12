using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

public class DapperConnectionProvider
{
    private readonly string _connectionString;

    public DapperConnectionProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}

