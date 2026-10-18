
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Infrastructure.Persistence;
using Products.IntegrationTests.Constants;
using Products.IntegrationTests.Infrastructure;
using Testcontainers.MsSql;

namespace Products.IntegrationTests;

[SetUpFixture]
public sealed class IntegrationTestFixture
{
    private static MsSqlContainer? _database;
    private static ProductsApiFactory? _factory;

    public static HttpClient Client { get; private set; } = null!;

    public static ProductsApiFactory Factory =>
        _factory ?? throw new InvalidOperationException(
            "Integration test infrastructure is not initialized.");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _database = new MsSqlBuilder(
                TestDatabaseSettings.SqlServerImage)
            .WithPassword(TestDatabaseSettings.SqlServerPassword)
            .Build();

        await _database.StartAsync();

        _factory = new ProductsApiFactory(
            _database.GetConnectionString());

        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ProductDbContext>();

        await context.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        Client?.Dispose();

        if (_factory is not null)
            await _factory.DisposeAsync();

        if (_database is not null)
            await _database.DisposeAsync();
    }

    public static async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ProductDbContext>();

        await context.Products.ExecuteDeleteAsync();
    }
}
