using OrderHub.Shared.Models;

namespace OrderHub.Persistence.Repositories;

public interface IOrderRepository
{
    Task<Guid> CreateOrderAsync(Orderr order);
    Task<IEnumerable<Orderr>> GetAllOrdersAsync();
    Task<bool> UpdateOrderAsync(Orderr order);
    Task<bool> DeleteOrderAsync(Guid id);
    Task<Orderr?> GetOrderByIdAsync(Guid orderId);

}
