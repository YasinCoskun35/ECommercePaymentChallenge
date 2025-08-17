using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;

namespace ECommerce.Infrastructure.Services;

public class BalanceManagementService : IBalanceManagementService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BalanceManagementService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BalanceManagementService(HttpClient httpClient, ILogger<BalanceManagementService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BalanceResponseDto> GetBalanceAsync()
    {
        try
        {
            _logger.LogInformation("Fetching balance from Balance Management service");
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api/balance");
            var context = new Context
            {
                ["logger"] = _logger,
                ["requestUri"] = httpRequest.RequestUri
            };
            httpRequest.SetPolicyExecutionContext(context);
            
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            
            var balanceResponse = await response.Content.ReadFromJsonAsync<BalanceResponseDto>(_jsonOptions);
            
            if (balanceResponse == null)
                throw new BalanceServiceException("GetBalance", "Received null response from Balance Management service");
            
            _logger.LogInformation("Successfully retrieved balance from Balance Management service");
            
            return balanceResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching balance");
            throw new BalanceServiceException("GetBalance", "HTTP request failed", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred while processing balance response");
            throw new BalanceServiceException("GetBalance", "Failed to parse balance response", ex);
        }
        catch (Exception ex) when (ex is not BalanceServiceException)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching balance");
            throw new BalanceServiceException("GetBalance", "Unexpected error occurred", ex);
        }
    }

    public async Task<IEnumerable<ExternalProductDto>> GetProductsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching products from Balance Management service");
            
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
            var context = new Context
            {
                ["logger"] = _logger,
                ["requestUri"] = request.RequestUri
            };
            request.SetPolicyExecutionContext(context);
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var productsResponse = await response.Content.ReadFromJsonAsync<ProductsResponseDto>(_jsonOptions);
            
            if (productsResponse?.Success == true && productsResponse.Data != null)
            {
                _logger.LogInformation("Successfully retrieved {Count} products from Balance Management service", 
                    productsResponse.Data.Count);
                return productsResponse.Data;
            }
            else
            {
                _logger.LogWarning("Balance Management service returned unsuccessful response or empty data");
                return new List<ExternalProductDto>();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching products from Balance Management service");
            throw new ApplicationException("Failed to retrieve products from Balance Management service", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred while processing products response");
            throw new ApplicationException("Failed to parse products response from Balance Management service", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching products");
            throw;
        }
    }

    public async Task<PreOrderResponseDto> CreatePreOrderAsync(PreOrderRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            _logger.LogInformation("Creating preorder with Balance Management service for order {OrderId} with amount {Amount}", 
                request.OrderId, request.Amount);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/balance/preorder")
            {
                Content = JsonContent.Create(request, options: _jsonOptions)
            };
            var context = new Context
            {
                ["logger"] = _logger,
                ["requestUri"] = httpRequest.RequestUri
            };
            httpRequest.SetPolicyExecutionContext(context);
            
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            
            var preorderResponse = await response.Content.ReadFromJsonAsync<PreOrderResponseDto>(_jsonOptions);
            
            if (preorderResponse == null)
                throw new ApplicationException("Received null response from Balance Management service");
            
            _logger.LogInformation("Successfully created preorder for order ID: {OrderId}", request.OrderId);
            
            return preorderResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating preorder with Balance Management service");
            throw new ApplicationException("Failed to create preorder with Balance Management service", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred while processing preorder response");
            throw new ApplicationException("Failed to parse preorder response from Balance Management service", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating preorder");
            throw;
        }
    }

    public async Task<CompleteOrderResponseDto> CompleteOrderAsync(CompleteOrderRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.OrderId))
            throw new ArgumentException("Order ID cannot be null or empty", nameof(request.OrderId));

        try
        {
            _logger.LogInformation("Completing order with Balance Management service for order ID: {OrderId}", request.OrderId);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/balance/complete")
            {
                Content = JsonContent.Create(request, options: _jsonOptions)
            };
            var context = new Context
            {
                ["logger"] = _logger,
                ["requestUri"] = httpRequest.RequestUri
            };
            httpRequest.SetPolicyExecutionContext(context);
            
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            
            var completeResponse = await response.Content.ReadFromJsonAsync<CompleteOrderResponseDto>(_jsonOptions);
            
            if (completeResponse == null)
                throw new ApplicationException("Received null response from Balance Management service");
            
            _logger.LogInformation("Successfully completed order with ID: {OrderId}", request.OrderId);
            
            return completeResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while completing order with Balance Management service");
            throw new ApplicationException($"Failed to complete order with order ID: {request.OrderId}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred while processing complete order response");
            throw new ApplicationException("Failed to parse complete order response from Balance Management service", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while completing order");
            throw;
        }
    }
} 