
namespace Products.Application.DTOs.Products;

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int InitialStock);
