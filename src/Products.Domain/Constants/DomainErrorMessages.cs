
using System.Reflection.Metadata.Ecma335;

namespace Products.Domain.Constants;

public static class DomainErrorMessages
{
    public static string Required(string fieldName) =>
        $"{fieldName} is required.";

    public static string MaxLength(string fieldName, int maxLength) =>
        $"{fieldName} cannot exceed {maxLength} characters.";

    public static string CannotBeNegative(string fieldName) =>
        $"{fieldName} cannot be negative.";

    public static string MustBePositive(string fieldName) =>
        $"{fieldName} must be greater than zero.";

    public const string InsufficientStock = "Insufficient stock.";

    public const string InvalidRowVersion = "Invalid product version.";
}