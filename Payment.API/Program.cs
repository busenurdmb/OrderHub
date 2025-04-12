using Payment.API.Messaging.Consumers;
using Payment.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

var app = builder.Build();


app.UseHttpsRedirection();

app.MapControllers();

app.Run();


