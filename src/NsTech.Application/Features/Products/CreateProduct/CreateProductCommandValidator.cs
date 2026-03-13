using FluentValidation;

namespace NsTech.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product Id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product Name is required.")
            .MaximumLength(100).WithMessage("Product Name must not exceed 100 characters.");
        RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Unit Price must be greater than 0.");
        RuleFor(x => x.AvailableQuantity).GreaterThanOrEqualTo(0).WithMessage("Available Quantity cannot be negative.");
    }
}
