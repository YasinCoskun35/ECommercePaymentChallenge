using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new order with product list and reserves funds
    /// </summary>
    /// <param name="request">The order creation request</param>
    /// <returns>The created order details wrapped in a standardized response</returns>
    /// <response code="201">Returns the newly created order</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="422">If there's insufficient stock, balance, or other business rule violations</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating new order for customer: {CustomerEmail}",
            request.CustomerEmail);
        
        var order = await _orderService.CreateOrderAsync(request);
        
        _logger.LogInformation("Successfully created order with ID: {OrderId}", order.Id);
        
        return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, 
            ApiResponse<OrderDto>.SuccessResult(order, "Order created successfully"));
    }

    /// <summary>
    /// Completes an order and finalizes payment
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <returns>The completed order details wrapped in a standardized response</returns>
    /// <response code="200">Returns the completed order</response>
    /// <response code="404">If the order was not found</response>
    /// <response code="422">If the order cannot be completed</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CompleteOrder(Guid id)
    {
        _logger.LogInformation("Completing order with ID: {OrderId}", id);
        
        var order = await _orderService.CompleteOrderAsync(id);
        
        _logger.LogInformation("Successfully completed order with ID: {OrderId}", id);
        
        return Ok(ApiResponse<OrderDto>.SuccessResult(order, "Order completed successfully"));
    }
} 