using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IBalanceManagementService _balanceManagementService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IBalanceManagementService balanceManagementService, ILogger<ProductService> logger)
    {
        _balanceManagementService = balanceManagementService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        _logger.LogInformation("Fetching products from Balance Management service");
        var externalProducts = await _balanceManagementService.GetProductsAsync();

        _logger.LogInformation("Successfully retrieved {Count} products from Balance Management service", externalProducts.Count());
        return externalProducts.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.Stock
        });
    }
} 