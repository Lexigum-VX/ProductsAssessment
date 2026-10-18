
namespace Products.Application.Exceptions;

public sealed class ProductValidationException(
    string message) : Exception(message);
