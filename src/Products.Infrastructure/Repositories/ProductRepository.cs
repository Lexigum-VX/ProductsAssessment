
using Microsoft.EntityFrameworkCore;
using Products.Application.Exceptions;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Repositories;

public sealed class ProductRepository(
    ProductDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStockRangeAsync(
        int min,
        int max,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Stock >= min && p.Stock <= max)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
    Product product,
    CancellationToken cancellationToken = default)
    {
        dbContext.Products.Remove(product);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProductConflictException(
                "The product was modified or deleted by another operation.");
        }
    }

    public async Task<int> AddStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(p =>
                p.Id == productId &&
                p.Stock <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        p => p.Stock,
                        p => p.Stock + quantity)
                    .SetProperty(
                        p => p.UpdatedAtUtc,
                        DateTime.UtcNow),
                cancellationToken);
    }

    public async Task<int> DecrementStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(p =>
                p.Id == productId &&
                p.Stock >= quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        p => p.Stock,
                        p => p.Stock - quantity)
                    .SetProperty(
                        p => p.UpdatedAtUtc,
                        DateTime.UtcNow),
                cancellationToken);
    }

    public async Task<Product?> GetByIdForUpdateAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task UpdateAsync(
        Product product,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default)
    {
        dbContext.Entry(product)
            .Property(p => p.RowVersion)
            .OriginalValue = expectedRowVersion;

        try
        {
            // Ensure that even an otherwise unchanged update
            // checks the expected concurrency token.
            dbContext.Entry(product)
                .Property(p => p.UpdatedAtUtc)
                .IsModified = true;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProductConflictException(
                "The product was modified by another operation.");
        }
    }
}
