using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace ECommerce.Tests.Unit.Services;

public class ProductServiceTests
{
    private readonly Mock<IBalanceManagementService> _mockBalanceManagementService;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockBalanceManagementService = new Mock<IBalanceManagementService>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        
        _productService = new ProductService(
            _mockBalanceManagementService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductsAsync_WhenBalanceManagementServiceReturnsProducts_ShouldReturnMappedProducts()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var externalProducts = new List<ExternalProductDto>
        {
            new()
            {
                Id = productId1.ToString(),
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 10
            },
            new()
            {
                Id = productId2.ToString(),
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50
            }
        };

        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(externalProducts);

        // Act
        var result = await _productService.GetProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var firstProduct = result.First();
        firstProduct.Id.Should().Be(productId1.ToString());
        firstProduct.Name.Should().Be("Laptop");
        firstProduct.Description.Should().Be("High-performance laptop");
        firstProduct.Price.Should().Be(999.99m);
        firstProduct.StockQuantity.Should().Be(10);
        
        var secondProduct = result.Last();
        secondProduct.Id.Should().Be(productId2.ToString());
        secondProduct.Name.Should().Be("Mouse");
        secondProduct.Description.Should().Be("Wireless mouse");
        secondProduct.Price.Should().Be(29.99m);
        secondProduct.StockQuantity.Should().Be(50);

        _mockBalanceManagementService.Verify(x => x.GetProductsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_WhenBalanceManagementServiceReturnsEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _mockBalanceManagementService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new List<ExternalProductDto>());

        // Act
        var result = await _productService.GetProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockBalanceManagementService.Verify(x => x.GetProductsAsync(), Times.Once);
    }
} 