using OrderHub.Shared.Events;

public interface IStockService
{
    Task HandleOrderCreatedAsync(StockReservedEvent orderEvent);
}
