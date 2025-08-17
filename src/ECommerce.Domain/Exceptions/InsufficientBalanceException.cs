namespace ECommerce.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public decimal RequiredAmount { get; }
    public decimal AvailableBalance { get; }
    
    public InsufficientBalanceException(decimal requiredAmount, decimal availableBalance) 
        : base($"Insufficient balance. Required: {requiredAmount}, Available: {availableBalance}")
    {
        RequiredAmount = requiredAmount;
        AvailableBalance = availableBalance;
    }
}
