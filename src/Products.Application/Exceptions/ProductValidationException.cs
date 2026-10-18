namespace Products.Application.Exceptions;

public sealed class ProductValidationException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);