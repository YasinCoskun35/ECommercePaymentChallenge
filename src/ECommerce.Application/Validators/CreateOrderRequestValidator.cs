using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address")
            .MaximumLength(255)
            .WithMessage("Customer email cannot exceed 255 characters");

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MaximumLength(200)
            .WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item")
            .Must(items => items != null && items.Any())
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100");
    }
} 