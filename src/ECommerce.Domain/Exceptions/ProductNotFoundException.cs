namespace ECommerce.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public string ProductId { get; }
    
    public ProductNotFoundException(string productId) 
        : base($"Product with ID {productId} was not found")
    {
        ProductId = productId;
    }
}
