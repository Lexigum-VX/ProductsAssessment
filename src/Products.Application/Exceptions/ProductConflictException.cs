
namespace Products.Application.Exceptions;

public sealed class ProductConflictException(
    string message) : Exception(message);
