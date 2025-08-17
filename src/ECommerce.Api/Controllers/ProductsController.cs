using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts()
    {
        _logger.LogInformation("Retrieving all products");
        
        var products = await _productService.GetProductsAsync();
        
        _logger.LogInformation("Successfully retrieved {Count} products", products.Count());
        
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products, $"Successfully retrieved {products.Count()} products"));
    }
} 