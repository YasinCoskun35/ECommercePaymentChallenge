using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        return services;
    }
}
