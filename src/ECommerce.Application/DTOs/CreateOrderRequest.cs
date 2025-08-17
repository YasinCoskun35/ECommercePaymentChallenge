using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

public class CreateOrderRequest
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
} 