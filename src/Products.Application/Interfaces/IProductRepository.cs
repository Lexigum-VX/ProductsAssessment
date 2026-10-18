using Products.Domain.Entities;

namespace Products.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByStockRangeAsync(
        int min,
        int max,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Product product,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<int> AddStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<int> DecrementStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdForUpdateAsync(
        int  id,
        CancellationToken cancellationToken = default)  ;
}
