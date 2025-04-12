using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OrderHub.Shared.Model;
using OrderHub.Shared.Models;
using StackExchange.Redis;
using System.Data;


namespace OrderHub.Persistence.DbContexts;

public class OrderDbContext : DbContext, IOutboxDbContext
{


    // EF Core için gerekli olan constructor
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }



    public DbSet<Orderr> Orders => Set<Orderr>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Paymentt> Payments => Set<Paymentt>();


}

