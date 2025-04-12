using Microsoft.AspNetCore.Mvc;
using OrderHub.Persistence.Repositories;
using OrderHub.Persistence.Services;
using OrderHub.Shared.DTOs;
using OrderHub.Shared.Models;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    public OrdersController(IOrderRepository orderRepository, IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderCreateRequest request)
    {
        _logger.LogInformation("📨 Yeni sipariş alındı: {Customer}", request.CustomerName);

        var id = await _orderService.CreateOrderAsync(request);

        _logger.LogInformation("✅ Sipariş başarıyla oluşturuldu: {OrderId}", id);

        return Ok(new { OrderId = id });

        //var order = new Orderr
        //{
        //    CustomerName = request.CustomerName,
        //    TotalPrice = request.TotalPrice
        //};

        //var id = await _orderRepository.CreateOrderAsync(order);
        //return Ok(new { OrderId = id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderRepository.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] Orderr order)
    {
        order.Id = id; // URL'den gelen ID'yi kullan
        var success = await _orderRepository.UpdateOrderAsync(order);
        return success ? Ok("Güncellendi") : NotFound("Kayıt bulunamadı");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var success = await _orderRepository.DeleteOrderAsync(id);
        return success ? Ok("Silindi") : NotFound("Kayıt bulunamadı");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }


}
