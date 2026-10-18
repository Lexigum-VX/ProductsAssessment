
using System.ComponentModel.DataAnnotations;
using Products.Domain.Constants;

namespace Products.Application.DTOs.Products;

public sealed class CreateProductRequest : IValidatableObject
{
    [Required]
    [MaxLength(ProductRules.NameMaxLength)]
    public required string Name { get; init; }

    [MaxLength(ProductRules.DescriptionMaxLength)]
    public string? Description { get; init; }

    public required decimal Price { get; init; }

    public required int InitialStock { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Price < ProductRules.MinimumPrice)
        {
            yield return new ValidationResult(
                DomainErrorMessages.CannotBeNegative(nameof(Price)),
                [nameof(Price)]);
        }

        if (InitialStock < ProductRules.MinimumStock)
        {
            yield return new ValidationResult(
                DomainErrorMessages.CannotBeNegative(nameof(InitialStock)),
                [nameof(InitialStock)]);
        }
    }
}
