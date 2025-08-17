using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseInMemoryDatabase("ECommerceDb"));

        services.AddHttpClient<IBalanceManagementService, BalanceManagementService>(client =>
        {
            client.BaseAddress = new Uri("https://balance-management-pi44.onrender.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalMilliseconds}ms");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalMilliseconds}ms. Last failure: {outcome?.Exception?.Message ?? outcome?.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("Circuit breaker half-open");
                });
    }
}

public static class ContextExtensions
{
    public static ILogger? GetLogger(this Context context)
    {
        return context.TryGetValue("logger", out var logger) ? logger as ILogger : null;
    }

    public static Uri? GetRequestUri(this Context context)
    {
        return context.TryGetValue("requestUri", out var uri) ? uri as Uri : null;
    }
}
