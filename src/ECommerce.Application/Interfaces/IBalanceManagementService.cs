using System.Text.Json.Serialization;

namespace ECommerce.Application.Interfaces;

public interface IBalanceManagementService
{
    Task<IEnumerable<ExternalProductDto>> GetProductsAsync();
    Task<BalanceResponseDto> GetBalanceAsync();
    Task<PreOrderResponseDto> CreatePreOrderAsync(PreOrderRequestDto request);
    Task<CompleteOrderResponseDto> CompleteOrderAsync(CompleteOrderRequestDto request);
}

public class ProductsResponseDto
{
    public bool Success { get; set; }
    public List<ExternalProductDto> Data { get; set; } = new();
}

public class ExternalProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class BalanceResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BalanceDataDto Data { get; set; } = new();
}

public class BalanceDataDto
{
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal BlockedBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class PreOrderRequestDto
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;
}

public class PreOrderResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PreOrderDataDto Data { get; set; } = new PreOrderDataDto();
}

public class PreOrderDataDto
{
    public PreOrderDto PreOrder { get; set; } = new PreOrderDto();
    public UpdatedBalanceDto UpdatedBalance { get; set; } = new UpdatedBalanceDto();
}

public class PreOrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class UpdatedBalanceDto
{
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal BlockedBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class CompleteOrderRequestDto
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;
}

public class CompleteOrderResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CompleteOrderDataDto Data { get; set; } = new CompleteOrderDataDto();
}

public class CompleteOrderDataDto
{
    public CompleteOrderDto Order { get; set; } = new CompleteOrderDto();
    public UpdatedBalanceDto UpdatedBalance { get; set; } = new UpdatedBalanceDto();
}

public class CompleteOrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
} 