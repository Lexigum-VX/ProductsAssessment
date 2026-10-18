
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Products.Application.DTOs.Products;
using Products.Domain.Constants;
using Products.IntegrationTests.Infrastructure;

namespace Products.IntegrationTests.Api;

[TestFixture]
public sealed class ProductsControllerTests : IntegrationTestBase
{
    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/products");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductResponse>>();

        Assert.That(products, Is.Not.Null);
        Assert.That(products, Is.Empty);
    }

    [Test]
    public async Task Create_ValidProduct_ReturnsCreated()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Contact Lenses",
            Description = "Soft contact lenses",
            Price = 29.99m,
            InitialStock = 100
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/products", request);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created));

        var product = await response.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.That(product, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(
                product!.Id,
                Is.InRange(
                    ProductRules.IdMinValue,
                    ProductRules.IdMaxValue));

            Assert.That(product.Name, Is.EqualTo(request.Name));
            Assert.That(product.Price, Is.EqualTo(request.Price));
            Assert.That(product.Stock, Is.EqualTo(request.InitialStock));

            Assert.That(
                response.Headers.Location?.ToString(),
                Does.Contain($"/api/products/{product.Id}"));
        });
    }

    [Test]
    public async Task Create_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Description = "Missing product name",
            Price = 29.99m,
            InitialStock = 100
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/products", request);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetById_NonExistingProduct_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync(
            $"/api/products/{ProductRules.IdMaxValue}");

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }


    [Test]
    public async Task Create_ValidProduct_CanBeRetrievedById()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Progressive Lenses",
            Description = "Multifocal lenses",
            Price = 249.99m,
            InitialStock = 50
        };

        // Act
        var createResponse = await Client.PostAsJsonAsync(
            "/api/products", request);

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.That(created, Is.Not.Null);

        var getResponse = await Client.GetAsync(
            $"/api/products/{created!.Id}");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var retrieved = await getResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved!.Id, Is.EqualTo(created.Id));
            Assert.That(retrieved.Name, Is.EqualTo(request.Name));
            Assert.That(retrieved.Stock, Is.EqualTo(request.InitialStock));
        });
    }

    [Test]
    public async Task Update_ValidRequest_UpdatesProduct()
    {
        // Arrange
        var created = await CreateProductAsync();

        var request = new UpdateProductRequest
        {
            Name = "Updated Contact Lenses",
            Description = "Updated description",
            Price = 39.99m,
            RowVersion = created.RowVersion
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/products/{created.Id}", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updated = await response.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Name, Is.EqualTo(request.Name));
            Assert.That(updated.Price, Is.EqualTo(request.Price));
            Assert.That(updated.Stock, Is.EqualTo(created.Stock));
            Assert.That(updated.RowVersion, Is.Not.EqualTo(created.RowVersion));
        });
    }

    [Test]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var created = await CreateProductAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync(
            $"/api/products/{created.Id}");

        var getResponse = await Client.GetAsync(
            $"/api/products/{created.Id}");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(
                deleteResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent));

            Assert.That(
                getResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    private async Task<ProductResponse> CreateProductAsync()
    {
        var request = new CreateProductRequest
        {
            Name = "Contact Lenses",
            Description = "Soft contact lenses",
            Price = 29.99m,
            InitialStock = 100
        };

        var response = await Client.PostAsJsonAsync(
            "/api/products", request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>()
            ?? throw new InvalidOperationException(
                "Product creation returned an empty response.");
    }


    [TestCase(29.999)]
    [TestCase(-1)]
    [TestCase(10000000000000000d)]
    public async Task Create_InvalidPrice_ReturnsBadRequest(
        double invalidPrice)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Contact Lenses",
                Price = invalidPrice,
                InitialStock = 10
            });

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ValidPriceWithTwoDecimalPlaces_ReturnsCreated()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest
            {
                Name = "Contact Lenses",
                Price = 29.99m,
                InitialStock = 10
            });

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created));
    }
}
