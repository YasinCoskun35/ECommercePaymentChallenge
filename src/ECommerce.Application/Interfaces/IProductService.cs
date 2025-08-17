using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
} 