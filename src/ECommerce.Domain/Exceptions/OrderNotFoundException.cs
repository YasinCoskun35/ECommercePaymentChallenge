namespace ECommerce.Domain.Exceptions;

public class OrderNotFoundException : DomainException
{
    public Guid OrderId { get; }
    
    public OrderNotFoundException(Guid orderId) 
        : base($"Order with ID {orderId} was not found")
    {
        OrderId = orderId;
    }
} 