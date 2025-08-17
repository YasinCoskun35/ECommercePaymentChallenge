using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ECommerceDbContext _context;

    public OrderRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
} 