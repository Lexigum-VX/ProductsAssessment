
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Products.Application.DTOs.Products;
using Products.IntegrationTests.Infrastructure;

namespace Products.IntegrationTests.Api;

[TestFixture]
public sealed class ProductsSearchAndStockTests : IntegrationTestBase
{
    private async Task<ProductResponse> CreateProductAsync(
        string name,
        int stock)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest
            {
                Name = name,
                Price = 29.99m,
                InitialStock = stock
            });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    [Test]
    public async Task Search_PartialName_ReturnsMatchingProducts()
    {
        await CreateProductAsync("Contact Lenses", 100);
        await CreateProductAsync("Toric Contact Lenses", 50);
        await CreateProductAsync("Progressive Lenses", 75);

        var response = await Client.GetAsync(
            "/api/products/search?name=Contact");

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductResponse>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(products, Has.Count.EqualTo(2));
            Assert.That(
                products!.All(p => p.Name.Contains("Contact")),
                Is.True);
        });
    }

    [Test]
    public async Task StockLevel_ValidRange_ReturnsMatchingProducts()
    {
        await CreateProductAsync("Contact Lenses", 100);
        await CreateProductAsync("Progressive Lenses", 50);
        await CreateProductAsync("Bifocal Lenses", 20);

        var response = await Client.GetAsync(
            "/api/products/stock-level?min=40&max=100");

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductResponse>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(products, Has.Count.EqualTo(2));
            Assert.That(
                products!.All(p => p.Stock >= 40 && p.Stock <= 100),
                Is.True);
        });
    }

    [Test]
    public async Task DecrementStock_ValidQuantity_UpdatesStock()
    {
        var product = await CreateProductAsync("Contact Lenses", 100);

        var response = await Client.PostAsync(
            $"/api/products/{product.Id}/decrement-stock/30", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updated = await Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}");

        Assert.That(updated!.Stock, Is.EqualTo(70));
    }

    [Test]
    public async Task DecrementStock_InsufficientStock_ReturnsConflict()
    {
        var product = await CreateProductAsync("Contact Lenses", 10);

        var response = await Client.PostAsync(
            $"/api/products/{product.Id}/decrement-stock/20", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var updated = await Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}");

        Assert.That(updated!.Stock, Is.EqualTo(10));
    }

    [Test]
    public async Task AddStock_ValidQuantity_UpdatesStock()
    {
        var product = await CreateProductAsync("Contact Lenses", 100);

        var response = await Client.PostAsync(
            $"/api/products/{product.Id}/add-to-stock/25", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updated = await Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}");

        Assert.That(updated!.Stock, Is.EqualTo(125));
    }
}
