using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
} 