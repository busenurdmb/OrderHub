using OrderHub.Shared.DTOs;
using OrderHub.Shared.Models;

namespace OrderHub.Persistence.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(OrderCreateRequest request);
    Task<Orderr?> GetOrderByIdAsync(Guid orderId);
}
