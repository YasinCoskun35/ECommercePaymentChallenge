using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto> CompleteOrderAsync(Guid orderId);
} 