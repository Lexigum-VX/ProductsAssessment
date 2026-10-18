
using NSubstitute;
using NUnit.Framework;
using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Domain.Entities;

namespace Products.UnitTests.Application;

[TestFixture]
public sealed class ProductServiceTests
{
    private IProductRepository _repository = null!;
    private ProductService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _service = new ProductService(_repository);
    }

    [Test]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = new Product(
            "Contact Lenses",
            "Soft contact lenses",
            29.99m,
            100);

        _repository.GetByIdAsync(
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _service.GetByIdAsync(100000);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo(product.Name));
            Assert.That(result.Stock, Is.EqualTo(product.Stock));
        });
    }

    [Test]
    public async Task GetByIdAsync_ProductNotFound_ReturnsNull()
    {
        // Arrange
        _repository.GetByIdAsync(
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        var result = await _service.GetByIdAsync(999999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DecrementStockAsync_Success_ReturnsSuccess()
    {
        // Arrange
        _repository.DecrementStockAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.DecrementStockAsync(100000, 10);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(Products.Application.Results.StockOperationResult.Success));

        await _repository.Received(1).DecrementStockAsync(
            100000,
            10,
            Arg.Any<CancellationToken>());
    }
}
