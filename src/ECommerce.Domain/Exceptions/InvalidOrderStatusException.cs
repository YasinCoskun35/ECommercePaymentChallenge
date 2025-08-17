namespace ECommerce.Domain.Exceptions;

public class InvalidOrderStatusException : DomainException
{
    public Guid OrderId { get; }
    public string CurrentStatus { get; }
    public string ExpectedStatus { get; }
    
    public InvalidOrderStatusException(Guid orderId, string currentStatus, string expectedStatus) 
        : base($"Order {orderId} cannot be completed. Current status: {currentStatus}, Expected: {expectedStatus}")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
        ExpectedStatus = expectedStatus;
    }
}
