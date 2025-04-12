namespace Payment.API.Services;

using OrderHub.Shared.Events;

public interface IPaymentService
{
    Task ProcessPaymentAsync(StockReservedEvent orderEvent);
}