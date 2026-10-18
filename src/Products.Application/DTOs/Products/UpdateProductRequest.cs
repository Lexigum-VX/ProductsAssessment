
using System.ComponentModel.DataAnnotations;
using Products.Domain.Constants;

namespace Products.Application.DTOs.Products;

public sealed class UpdateProductRequest : IValidatableObject
{
    [Required]
    [MaxLength(ProductRules.NameMaxLength)]
    public required string Name { get; init; }

    [MaxLength(ProductRules.DescriptionMaxLength)]
    public string? Description { get; init; }

    public required decimal Price { get; init; }

    [Required]
    public required string RowVersion { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Price < ProductRules.MinimumPrice)
        {
            yield return new ValidationResult(
                DomainErrorMessages.CannotBeNegative(nameof(Price)),
                [nameof(Price)]);
        }

        if (Price > ProductRules.MaximumPrice)
        {
            yield return new ValidationResult(
                DomainErrorMessages.MaximumValue(
                    nameof(Price),
                    ProductRules.MaximumPrice),
                [nameof(Price)]);
        }

        if (!ProductRules.HasValidPricePrecision(Price))
        {
            yield return new ValidationResult(
                DomainErrorMessages.MaximumDecimalPlaces(
                    nameof(Price),
                    ProductRules.PriceDecimalPlaces),
                [nameof(Price)]);
        }

        if (!IsValidRowVersion(RowVersion))
        {
            yield return new ValidationResult(
                DomainErrorMessages.InvalidRowVersion,
                [nameof(RowVersion)]);
        }
    }

    private static bool IsValidRowVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        Span<byte> buffer =
            stackalloc byte[ProductRules.RowVersionLength];

        return Convert.TryFromBase64String(
            value,
            buffer,
            out var bytesWritten)
            && bytesWritten == ProductRules.RowVersionLength;
    }
}
