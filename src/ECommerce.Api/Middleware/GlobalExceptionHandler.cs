using System.Net;
using System.Text.Json;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Http.Extensions;

namespace ECommerce.Api.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<object> errorResponse;

        switch (exception)
        {
            case ProductNotFoundException productEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.ErrorResult("Product not found", productEx.Message);
                _logger.LogWarning(productEx, "Product not found: {ProductId}", productEx.ProductId);
                break;

            case InsufficientStockException stockEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = ApiResponse<object>.ErrorResult("Insufficient stock", stockEx.Message);
                _logger.LogWarning(stockEx, "Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}", 
                    stockEx.ProductId, stockEx.RequestedQuantity, stockEx.AvailableStock);
                break;

            case InsufficientBalanceException balanceEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = ApiResponse<object>.ErrorResult("Insufficient balance", balanceEx.Message);
                _logger.LogWarning(balanceEx, "Insufficient balance. Required: {Required}, Available: {Available}",
                    balanceEx.RequiredAmount, balanceEx.AvailableBalance);
                break;

            case OrderNotFoundException orderEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ApiResponse<object>.ErrorResult("Order not found", orderEx.Message);
                _logger.LogWarning(orderEx, "Order not found: {OrderId}", orderEx.OrderId);
                break;

            case InvalidOrderStatusException statusEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = ApiResponse<object>.ErrorResult("Invalid order status", statusEx.Message);
                _logger.LogWarning(statusEx, "Invalid order status for order {OrderId}. Current: {Current}, Expected: {Expected}", 
                    statusEx.OrderId, statusEx.CurrentStatus, statusEx.ExpectedStatus);
                break;

            case BalanceServiceException balanceServiceEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = ApiResponse<object>.ErrorResult("Balance service error", balanceServiceEx.Message);
                _logger.LogWarning(balanceServiceEx, "Balance service error for operation {Operation}",
                    balanceServiceEx.Operation);
                break;

            case DomainException domainEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.ErrorResult("Domain validation error", domainEx.Message);
                _logger.LogWarning(domainEx, "Domain exception occurred: {Message}", domainEx.Message);
                break;

            case ArgumentNullException argNullEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.ErrorResult("Missing required parameter", argNullEx.Message);
                _logger.LogWarning(argNullEx, "Missing required parameter: {Message}", argNullEx.Message);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.ErrorResult("Validation error", argEx.Message);
                _logger.LogWarning(argEx, "Validation exception occurred: {Message}", argEx.Message);
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = ApiResponse<object>.ErrorResult("Business rule violation", invalidOpEx.Message);
                _logger.LogWarning(invalidOpEx, "Business rule violation: {Message}", invalidOpEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = ApiResponse<object>.ErrorResult("An unexpected error occurred", exception.Message);
                _logger.LogError(exception, "Unexpected exception occurred: {Message}", exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
} 