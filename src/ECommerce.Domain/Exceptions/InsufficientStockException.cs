namespace ECommerce.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public string ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }
    
    public InsufficientStockException(string productId, int requestedQuantity, int availableStock) 
        : base($"Insufficient stock for product {productId}. Requested: {requestedQuantity}, Available: {availableStock}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
    }
} 