using ECommerce.Domain.ValueObjects;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerEmail { get; private set; }
    public string CustomerName { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? PaymentReference { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public ICollection<OrderItem> OrderItems { get; private set; }

    private Order()
    {
        OrderItems = new List<OrderItem>();
    }
    public Order(string customerEmail, string customerName, decimal totalAmount, ICollection<OrderItem> orderItems)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email cannot be empty", nameof(customerEmail));
        
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));
        
        if (totalAmount <= 0)
            throw new ArgumentException("Total amount must be greater than zero", nameof(totalAmount));
        
        if (orderItems == null || !orderItems.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(orderItems));

        Id = Guid.NewGuid();
        CustomerEmail = customerEmail;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.Created;
        OrderItems = orderItems;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReservePayment(string paymentReference)
    {
        if (!Status.CanTransitionTo(OrderStatus.PaymentReserved))
            throw new InvalidOrderStatusException(Id, Status.Value, OrderStatus.PaymentReserved.Value);

        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("Payment reference cannot be empty", nameof(paymentReference));

        Status = OrderStatus.PaymentReserved;
        PaymentReference = paymentReference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (!Status.CanTransitionTo(OrderStatus.Completed))
            throw new InvalidOrderStatusException(Id, Status.Value, OrderStatus.Completed.Value);

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (!Status.CanTransitionTo(OrderStatus.Failed))
            throw new InvalidOrderStatusException(Id, Status.Value, OrderStatus.Failed.Value);

        Status = OrderStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }


} 