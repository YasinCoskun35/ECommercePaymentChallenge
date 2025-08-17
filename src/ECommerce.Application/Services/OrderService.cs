using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBalanceManagementService _balanceManagementService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IBalanceManagementService balanceManagementService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _balanceManagementService = balanceManagementService ?? throw new ArgumentNullException(nameof(balanceManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        ValidateCreateOrderRequest(request);

        var orderItems = await ValidateAndCreateOrderItemsAsync(request.Items);
        var totalAmount = CalculateTotalAmount(orderItems);
        
        await ValidateBalanceAsync(totalAmount);

        var order = new Order(request.CustomerEmail, request.CustomerName, totalAmount, orderItems);
        var savedOrder = await _orderRepository.AddAsync(order);

        await ProcessPaymentAsync(savedOrder, totalAmount);

        _logger.LogInformation("Successfully created order {OrderId} with total amount {TotalAmount}", 
            savedOrder.Id, totalAmount);

        return MapToOrderDto(savedOrder);
    }

    public async Task<OrderDto> CompleteOrderAsync(Guid orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        
        await CompletePaymentAsync(order);
        
        var completedOrder = await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Successfully completed order {OrderId}", orderId);

        return MapToOrderDto(completedOrder);
    }

    private static void ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(request.Items));
    }

    private async Task<ICollection<OrderItem>> ValidateAndCreateOrderItemsAsync(IEnumerable<OrderItemRequest> items)
    {
        var externalProducts = await _balanceManagementService.GetProductsAsync();
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var externalProduct = externalProducts.FirstOrDefault(p => p.Id == item.ProductId);
            if (externalProduct == null)
                throw new ProductNotFoundException(item.ProductId);

            if (item.Quantity <= 0)
                throw new ArgumentException($"Quantity must be greater than 0 for product {item.ProductId}", nameof(item.Quantity));

            if (externalProduct.Stock < item.Quantity)
                throw new InsufficientStockException(item.ProductId, item.Quantity, externalProduct.Stock);

            var itemTotal = externalProduct.Price * item.Quantity;
            orderItems.Add(new OrderItem
            {
                ProductId = externalProduct.Id, // Use the external product ID as string
                ProductName = externalProduct.Name,
                Quantity = item.Quantity,
                UnitPrice = externalProduct.Price,
                TotalPrice = itemTotal
            });
        }

        return orderItems;
    }

    private static decimal CalculateTotalAmount(ICollection<OrderItem> orderItems)
    {
        return orderItems.Sum(item => item.TotalPrice);
    }

    private async Task ValidateBalanceAsync(decimal totalAmount)
    {
        var balanceResponse = await _balanceManagementService.GetBalanceAsync();
        
        if (!balanceResponse.Success)
            throw new BalanceServiceException("GetBalance", "Failed to retrieve balance information");

        if (balanceResponse.Data.AvailableBalance < totalAmount)
            throw new InsufficientBalanceException(totalAmount, balanceResponse.Data.AvailableBalance);
    }

    private async Task ProcessPaymentAsync(Order order, decimal totalAmount)
    {
        var preorderRequest = new PreOrderRequestDto
        {
            OrderId = order.Id.ToString(),
            Amount = totalAmount
        };

        var preorderResponse = await _balanceManagementService.CreatePreOrderAsync(preorderRequest);

        if (!preorderResponse.Success)
        {
            order.Fail();
            await _orderRepository.UpdateAsync(order);
            throw new BalanceServiceException("CreatePreOrder", preorderResponse.Message);
        }

        order.ReservePayment(preorderResponse.Data.PreOrder.OrderId);
        await _orderRepository.UpdateAsync(order);
    }

    private async Task<Order> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new OrderNotFoundException(orderId);

        return order;
    }

    private async Task CompletePaymentAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.PaymentReference))
            throw new InvalidOperationException($"Order {order.Id} has no payment reference");

        var completeRequest = new CompleteOrderRequestDto
        {
            OrderId = order.PaymentReference
        };

        var completeResponse = await _balanceManagementService.CompleteOrderAsync(completeRequest);

        if (!completeResponse.Success)
            throw new BalanceServiceException("CompleteOrder", completeResponse.Message);

        order.Complete();
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id.ToString(),
            CustomerEmail = order.CustomerEmail,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            Status = order.Status.Value,
            PaymentReference = order.PaymentReference,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId.ToString(),
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
} 