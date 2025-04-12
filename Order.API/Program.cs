using Microsoft.EntityFrameworkCore;
using OrderHub.Infrastructure.Logging.Helper;
using OrderHub.Infrastructure.Messaging;
using OrderHub.Infrastructure.Outbox;
using OrderHub.Persistence.DbContexts;
using OrderHub.Persistence.Repositories;
using OrderHub.Persistence.Services;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    sp => ConnectionMultiplexer.Connect("localhost:6381")); // docker-compose'daki ayara göre

builder.Services.AddSingleton<IRedisService, RedisService>();


Log.Logger = new LoggerConfiguration()
     .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "orderhub-logs-{0:yyyy.MM.dd}",
        CustomFormatter = new SimpleElasticsearchFormatter()
    })
    .CreateLogger();

builder.Host.UseSerilog();


// DI
builder.Services.AddScoped<DapperConnectionProvider>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IOutboxDbContext>(provider =>
    provider.GetRequiredService<OrderDbContext>());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Recipe API v1");
        c.RoutePrefix = "swagger"; // "/swagger" yolunu kullan
    });
}

app.UseHttpsRedirection();



app.MapControllers();


app.Run();

