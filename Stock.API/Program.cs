using OrderHub.Infrastructure.Messaging;
using Stock.API.Mesaging.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

builder.Services.AddHostedService<OrderCreatedConsumer>();

var app = builder.Build();

app.UseHttpsRedirection();


app.MapControllers();

app.Run();


