
using Products.Application.DTOs.Products;
using Products.Application.Exceptions;
using Products.Application.Interfaces;
using Products.Application.Mappings;
using Products.Application.Results;
using Products.Domain.Constants;
using Products.Domain.Entities;

namespace Products.Application.Services;

public sealed class ProductService(
    IProductRepository productRepository) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(
            cancellationToken);

        return products.Select(p => p.ToResponse()).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
            id, cancellationToken);

        return product?.ToResponse();
    }


    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        Product product;

        try
        {
            product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.InitialStock);
        }
        catch (ArgumentException ex)
        {
            throw new ProductValidationException(ex.Message, ex);
        }

        await productRepository.AddAsync(
            product, cancellationToken);

        return product.ToResponse();
    }


    public async Task<ProductResponse?> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var expectedRowVersion = ParseRowVersion(request.RowVersion);

        var product = await productRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (product is null)
            return null;

        try
        {
            product.UpdateDetails(
                request.Name,
                request.Description,
                request.Price);
        }
        catch (ArgumentException ex)
        {
            throw new ProductValidationException(ex.Message, ex);
        }

        await productRepository.UpdateAsync(
            product,
            expectedRowVersion,
            cancellationToken);

        return product.ToResponse();
    }


    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (product is null)
            return false;

        await productRepository.DeleteAsync(
            product, cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<ProductResponse>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ProductValidationException(
                DomainErrorMessages.Required(nameof(name)));

        var products = await productRepository.SearchByNameAsync(
            name.Trim(), cancellationToken);

        return products.Select(p => p.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetByStockRangeAsync(
        int min,
        int max,
        CancellationToken cancellationToken = default)
    {
        if (min < ProductRules.MinimumStock)
            throw new ProductValidationException(
                DomainErrorMessages.CannotBeNegative(nameof(min)));

        if (max < min)
            throw new ProductValidationException(
                DomainErrorMessages.InvalidStockRange);

        var products = await productRepository.GetByStockRangeAsync(
            min, max, cancellationToken);

        return products.Select(p => p.ToResponse()).ToList();
    }

    private static byte[] ParseRowVersion(string rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
            throw new ProductValidationException(
                DomainErrorMessages.Required(nameof(rowVersion)));

        try
        {
            var bytes = Convert.FromBase64String(rowVersion);

            if (bytes.Length != ProductRules.RowVersionLength)
                throw new ProductValidationException(
                    DomainErrorMessages.InvalidRowVersion);

            return bytes;
        }
        catch (FormatException)
        {
            throw new ProductValidationException(
                DomainErrorMessages.InvalidRowVersion);
        }
    }


    public async Task<StockOperationResult> AddStockAsync(
        int id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        ValidateStockQuantity(quantity);

        var affectedRows = await productRepository.AddStockAsync(
            id, quantity, cancellationToken);

        return await ResolveStockOperationAsync(
            id, affectedRows, cancellationToken);
    }

    public async Task<StockOperationResult> DecrementStockAsync(
        int id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        ValidateStockQuantity(quantity);

        var affectedRows = await productRepository.DecrementStockAsync(
            id, quantity, cancellationToken);

        return await ResolveStockOperationAsync(
            id, affectedRows, cancellationToken);
    }

    private static void ValidateStockQuantity(int quantity)
    {
        if (quantity < ProductRules.MinimumStockAdjustment)
            throw new ProductValidationException(
                DomainErrorMessages.MustBePositive(nameof(quantity)));
    }

    private async Task<StockOperationResult> ResolveStockOperationAsync(
        int id,
        int affectedRows,
        CancellationToken cancellationToken)
    {
        if (affectedRows > 0)
            return StockOperationResult.Success;

        var product = await productRepository.GetByIdAsync(
            id, cancellationToken);

        return product is null
            ? StockOperationResult.ProductNotFound
            : StockOperationResult.Conflict;
    }

}
