namespace ECommerce.Domain.Exceptions;

public class BalanceServiceException : DomainException
{
    public string Operation { get; }
    
    public BalanceServiceException(string operation, string message) 
        : base($"Balance service operation '{operation}' failed: {message}")
    {
        Operation = operation;
    }
    
    public BalanceServiceException(string operation, string message, Exception innerException) 
        : base($"Balance service operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }
}
