
using OrderHub.Infrastructure.Messaging;
using OrderHub.Infrastructure.Outbox;
using OrderHub.Persistence.Repositories;
using OrderHub.Shared.DTOs;
using OrderHub.Shared.Events;
using OrderHub.Shared.Model;
using OrderHub.Shared.Models;

namespace OrderHub.Persistence.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxService _outboxService;
    private readonly IRedisService _redisService;

    public OrderService(IOrderRepository orderRepository, IOutboxService outboxService, IRedisService redisService)
    {
        _orderRepository = orderRepository;
        _outboxService = outboxService;
        _redisService = redisService;
    }

    //private readonly IEventPublisher _eventPublisher;


    public async Task<Guid> CreateOrderAsync(OrderCreateRequest request)
    {
        var orderItems = request.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity
        }).ToList();

        var order = new Orderr
        {
            CustomerName = request.CustomerName,
            TotalPrice = request.TotalPrice,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        var id = await _orderRepository.CreateOrderAsync(order);
        await _redisService.SetAsync($"order:{order.Id}", order, TimeSpan.FromMinutes(30));


        // Outbox mesajı burada yazılır
        var evt = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemMessage
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };
        await _outboxService.AddMessageAsync(evt, nameof(OrderCreatedEvent));
        //await _eventPublisher.PublishAsync(@event, "order-created-queue");

        return id;
    }
    public async Task<Orderr?> GetOrderByIdAsync(Guid orderId)
    {
        // 🔍 Önce Redis'ten kontrol et
        var cacheKey = $"order:{orderId}";
        var cachedOrder = await _redisService.GetAsync<Orderr>(cacheKey);

        if (cachedOrder is not null)
        {
            Console.WriteLine("✅ Sipariş Redis cache'ten getirildi.");
            return cachedOrder;
        }

        // 📦 Yoksa veritabanından getir
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order is not null)
        {
            // ♻️ Redis'e yaz (opsiyonel: süresiyle birlikte)
            await _redisService.SetAsync(cacheKey, order, TimeSpan.FromMinutes(30));
        }

        return order;
    }
}
