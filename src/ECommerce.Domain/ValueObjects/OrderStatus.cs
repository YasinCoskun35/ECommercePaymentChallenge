namespace ECommerce.Domain.ValueObjects;

public class OrderStatus : IEquatable<OrderStatus>
{
    public static readonly OrderStatus Created = new("Created");
    public static readonly OrderStatus PaymentReserved = new("PaymentReserved");
    public static readonly OrderStatus Completed = new("Completed");
    public static readonly OrderStatus Failed = new("Failed");
    public static readonly OrderStatus Cancelled = new("Cancelled");

    public string Value { get; }

    private OrderStatus(string value)
    {
        Value = value;
    }

    public static OrderStatus FromString(string value)
    {
        return value?.ToLower() switch
        {
            "created" => Created,
            "paymentreserved" => PaymentReserved,
            "completed" => Completed,
            "failed" => Failed,
            "cancelled" => Cancelled,
            _ => throw new ArgumentException($"Invalid order status: {value}")
        };
    }

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return Value switch
        {
            "Created" => targetStatus.Value is "PaymentReserved" or "Failed" or "Cancelled",
            "PaymentReserved" => targetStatus.Value is "Completed" or "Failed" or "Cancelled",
            "Completed" => false, // Terminal state
            "Failed" => false, // Terminal state
            "Cancelled" => false, // Terminal state
            _ => false
        };
    }

    public bool Equals(OrderStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as OrderStatus);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(OrderStatus? left, OrderStatus? right)
    {
        return EqualityComparer<OrderStatus>.Default.Equals(left, right);
    }

    public static bool operator !=(OrderStatus? left, OrderStatus? right)
    {
        return !(left == right);
    }
}
