using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Tests.Integration.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
    }

    [Fact]
    public void ProductsController_ShouldBeCreated()
    {
        // Arrange & Act
        var controller = new ECommerce.Api.Controllers.ProductsController(
            _mockProductService.Object, 
            Mock.Of<ILogger<ECommerce.Api.Controllers.ProductsController>>());

        // Assert
        controller.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducts_WhenProductsExist_ShouldReturnProducts()
    {
        // Arrange
        var expectedProducts = new List<ProductDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                StockQuantity = 50
            }
        };

        _mockProductService
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(expectedProducts);

        var controller = new ECommerce.Api.Controllers.ProductsController(
            _mockProductService.Object, 
            Mock.Of<ILogger<ECommerce.Api.Controllers.ProductsController>>());

        // Act
        var result = await controller.GetProducts();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        
        var apiResponse = okResult!.Value as ApiResponse<IEnumerable<ProductDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().HaveCount(1);
    }
} 