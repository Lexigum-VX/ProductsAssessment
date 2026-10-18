
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Products.Application.DTOs.Products;
using Products.Domain.Constants;
using Products.IntegrationTests.Infrastructure;

namespace Products.IntegrationTests.Api;

[TestFixture]
public sealed class ProductsConcurrencyTests : IntegrationTestBase
{
    private const int ConcurrentRequests = 20;

    [Test]
    public async Task Create_ConcurrentRequests_GeneratesUniqueIds()
    {
        // Arrange
        var requests = Enumerable.Range(0, ConcurrentRequests)
            .Select(index => new CreateProductRequest
            {
                Name = $"Contact Lenses {index}",
                Description = "Concurrency test product",
                Price = 29.99m,
                InitialStock = 100
            });

        // Act
        var responses = await Task.WhenAll(
            requests.Select(request =>
                Client.PostAsJsonAsync("/api/products", request)));

        // Assert
        Assert.That(
            responses.All(r => r.StatusCode == HttpStatusCode.Created),
            Is.True,
            "All products should be created successfully.");

        var products = await Task.WhenAll(
            responses.Select(async response =>
                await response.Content
                    .ReadFromJsonAsync<ProductResponse>()));

        var ids = products.Select(p => p!.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(ids, Has.Length.EqualTo(ConcurrentRequests));

            Assert.That(
                ids.Distinct().Count(),
                Is.EqualTo(ConcurrentRequests));

            Assert.That(
                ids.All(id =>
                    id >= ProductRules.IdMinValue &&
                    id <= ProductRules.IdMaxValue),
                Is.True);
        });
    }

    [Test]
    public async Task DecrementStock_ConcurrentRequests_PreventsNegativeStock()
    {
        // Arrange
        var product = await CreateProductAsync(initialStock: 10);

        // Act
        var responses = await Task.WhenAll(
            Client.PostAsync(
                $"/api/products/{product.Id}/decrement-stock/7",
                null),
            Client.PostAsync(
                $"/api/products/{product.Id}/decrement-stock/6",
                null));

        // Assert
        Assert.That(
            responses.Count(r => r.StatusCode == HttpStatusCode.OK),
            Is.EqualTo(1));

        Assert.That(
            responses.Count(r => r.StatusCode == HttpStatusCode.Conflict),
            Is.EqualTo(1));

        var updated = await Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}");

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Stock, Is.AnyOf(3, 4));
    }

    [Test]
    public async Task Update_StaleRowVersion_ReturnsConflict()
    {
        // Arrange
        var product = await CreateProductAsync(initialStock: 100);

        var firstRequest = new UpdateProductRequest
        {
            Name = "Updated Contact Lenses",
            Description = "First update",
            Price = 39.99m,
            RowVersion = product.RowVersion
        };

        var staleRequest = new UpdateProductRequest
        {
            Name = "Outdated Contact Lenses",
            Description = "Second update",
            Price = 49.99m,
            RowVersion = product.RowVersion
        };


        // Act
        var firstResponse = await Client.PutAsJsonAsync(
            $"/api/products/{product.Id}",
            firstRequest);

        var secondResponse = await Client.PutAsJsonAsync(
            $"/api/products/{product.Id}",
            staleRequest);

        var responseBody = await secondResponse.Content
            .ReadAsStringAsync();

        TestContext.Out.WriteLine(
            $"Second PUT: {(int)secondResponse.StatusCode}");

        TestContext.Out.WriteLine(responseBody);

        // Assert
        Assert.That(
            firstResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        Assert.That(
            secondResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Conflict));

        var current = await Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}");

        Assert.Multiple(() =>
        {
            Assert.That(
                current!.Name,
                Is.EqualTo(firstRequest.Name));

            Assert.That(
                current.Price,
                Is.EqualTo(firstRequest.Price));
        });
    }

    private async Task<ProductResponse> CreateProductAsync(int initialStock)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest
            {
                Name = "Contact Lenses",
                Description = "Concurrency test",
                Price = 29.99m,
                InitialStock = initialStock
            });

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>()
            ?? throw new InvalidOperationException(
                "Product creation returned an empty response.");
    }
}
