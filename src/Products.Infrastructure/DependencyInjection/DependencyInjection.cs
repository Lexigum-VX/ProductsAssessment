using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Persistence.Seed;

namespace Products.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ProductsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'ProductsDb' is not configured.");

        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(ProductDbContext).Assembly.FullName));

            options.UseSeeding((context, _) =>
            {
                ProductSeeder.Seed((ProductDbContext)context);
            });

            options.UseAsyncSeeding(async (
                context, _, cancellationToken) =>
            {
                await ProductSeeder.SeedAsync(
                    (ProductDbContext)context,
                    cancellationToken);
            });
        });

        return services;
    }
}
