
using Microsoft.EntityFrameworkCore;
using Products.Domain.Constants;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence;

public sealed class ProductDbContext(
    DbContextOptions<ProductDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<int>("ProductIds", "dbo")
            .StartsAt(ProductRules.IdMinValue)
            .IncrementsBy(1)
            .HasMin(ProductRules.IdMinValue)
            .HasMax(ProductRules.IdMaxValue)
            .IsCyclic(false);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProductDbContext).Assembly);
    }
}
