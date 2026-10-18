
namespace Products.Application.DTOs.Products;

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string RowVersion);
