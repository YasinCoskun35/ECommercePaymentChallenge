using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace ECommerce.Tests.Unit.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IBalanceManagementService> _mockBalanceManagementService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockBalanceManagementService = new Mock<IBalanceManagementService>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        
        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockBalanceManagementService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var productId = "PROD-001";
        var externalProduct = new ExternalProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10
        };

        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Quantity = 2 }
            }
        };

        var balanceResponse = new BalanceResponseDto
        {
            Success = true,
            Data = new BalanceDataDto
            {
                AvailableBalance = 5000m
            }
        };

        var orderItems = new List<OrderItem>
        {
            new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 2, UnitPrice = 999.99m, TotalPrice = 1999.98m }
        };

        var savedOrder = new Order("test@example.com", "John Doe", 1999.98m, orderItems);

        var preorderResponse = new PreOrderResponseDto
        {
            Success = true,
            Data = new PreOrderDataDto
            {
                PreOrder = new PreOrderDto
                {
                    OrderId = savedOrder.Id.ToString(),
                    Amount = 1999.98m,
                    Status = "blocked",
                    Timestamp = DateTime.UtcNow
                }
            }
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto> { externalProduct });

        _mockBalanceManagementService
            .Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(balanceResponse);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(savedOrder);

        _mockBalanceManagementService
            .Setup(x => x.CreatePreOrderAsync(It.IsAny<PreOrderRequestDto>()))
            .ReturnsAsync(preorderResponse);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(savedOrder);

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerEmail.Should().Be(request.CustomerEmail);
        result.CustomerName.Should().Be(request.CustomerName);
        result.TotalAmount.Should().Be(1999.98m);
        result.Status.Should().Be(OrderStatus.PaymentReserved.Value);
        result.PaymentReference.Should().Be(savedOrder.Id.ToString());

        _mockBalanceManagementService.Verify(x => x.GetProductsAsync(), Times.Once);
        _mockBalanceManagementService.Verify(x => x.GetBalanceAsync(), Times.Once);
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _mockBalanceManagementService.Verify(x => x.CreatePreOrderAsync(It.IsAny<PreOrderRequestDto>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyItems_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(request));
        
        exception.Message.Should().Contain("Order must contain at least one item");
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentProduct_ShouldThrowProductNotFoundException()
    {
        // Arrange
        var productId = "PROD-001";
        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _orderService.CreateOrderAsync(request));
        
        exception.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var productId = "PROD-001";
        var externalProduct = new ExternalProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 1
        };

        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Quantity = 5 }
            }
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto> { externalProduct });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _orderService.CreateOrderAsync(request));
        
        exception.ProductId.Should().Be(productId);
        exception.RequestedQuantity.Should().Be(5);
        exception.AvailableStock.Should().Be(1);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientBalance_ShouldThrowInsufficientBalanceException()
    {
        // Arrange
        var productId = "PROD-001";
        var externalProduct = new ExternalProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10
        };

        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        var balanceResponse = new BalanceResponseDto
        {
            Success = true,
            Data = new BalanceDataDto
            {
                AvailableBalance = 500m // Less than required 999.99m
            }
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto> { externalProduct });

        _mockBalanceManagementService
            .Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(balanceResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientBalanceException>(
            () => _orderService.CreateOrderAsync(request));
        
        exception.RequiredAmount.Should().Be(999.99m);
        exception.AvailableBalance.Should().Be(500m);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenPreorderFails_ShouldThrowBalanceServiceException()
    {
        // Arrange
        var productId = "PROD-001";
        var externalProduct = new ExternalProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10
        };

        var request = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        var balanceResponse = new BalanceResponseDto
        {
            Success = true,
            Data = new BalanceDataDto
            {
                AvailableBalance = 5000m
            }
        };

        var orderItems = new List<OrderItem>
        {
            new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m, TotalPrice = 999.99m }
        };

        var savedOrder = new Order("test@example.com", "John Doe", 999.99m, orderItems);

        var preorderResponse = new PreOrderResponseDto
        {
            Success = false,
            Message = "Preorder failed"
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto> { externalProduct });

        _mockBalanceManagementService
            .Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(balanceResponse);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(savedOrder);

        _mockBalanceManagementService
            .Setup(x => x.CreatePreOrderAsync(It.IsAny<PreOrderRequestDto>()))
            .ReturnsAsync(preorderResponse);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(savedOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BalanceServiceException>(
            () => _orderService.CreateOrderAsync(request));
        
        exception.Operation.Should().Be("CreatePreOrder");
        exception.Message.Should().Contain("Preorder failed");
    }

    [Fact]
    public async Task CompleteOrderAsync_WithValidOrder_ShouldCompleteOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>
        {
            new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m, TotalPrice = 999.99m }
        };

        var order = new Order("test@example.com", "John Doe", 999.99m, orderItems);
        order.ReservePayment("PAY-123");

        var completeResponse = new CompleteOrderResponseDto
        {
            Success = true,
            Data = new CompleteOrderDataDto
            {
                Order = new CompleteOrderDto
                {
                    OrderId = "PAY-123",
                    Status = "completed"
                }
            }
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockBalanceManagementService
            .Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequestDto>()))
            .ReturnsAsync(completeResponse);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CompleteOrderAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Completed.Value);

        _mockOrderRepository.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _mockBalanceManagementService.Verify(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequestDto>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CompleteOrderAsync_WithNonExistentOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => _orderService.CompleteOrderAsync(orderId));
        
        exception.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task CompleteOrderAsync_WithMissingPaymentReference_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>
        {
            new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m, TotalPrice = 999.99m }
        };

        var order = new Order("test@example.com", "John Doe", 999.99m, orderItems);
        // Order starts with Created status and no payment reference

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orderService.CompleteOrderAsync(orderId));
        
        exception.Message.Should().Contain("has no payment reference");
    }

    [Fact]
    public async Task CompleteOrderAsync_WhenCompleteOrderFails_ShouldThrowBalanceServiceException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>
        {
            new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m, TotalPrice = 999.99m }
        };

        var order = new Order("test@example.com", "John Doe", 999.99m, orderItems);
        order.ReservePayment("PAY-123");

        var completeResponse = new CompleteOrderResponseDto
        {
            Success = false,
            Message = "Order completion failed"
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockBalanceManagementService
            .Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequestDto>()))
            .ReturnsAsync(completeResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BalanceServiceException>(
            () => _orderService.CompleteOrderAsync(orderId));
        
        exception.Operation.Should().Be("CompleteOrder");
        exception.Message.Should().Contain("Order completion failed");
    }
} 