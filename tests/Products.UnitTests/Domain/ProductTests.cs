using Products.Domain.Entities;

namespace Products.UnitTests.Domain;

[TestFixture]
public sealed class ProductTests
{
    private Product _product = null!;

    [SetUp]
    public void SetUp()
    {
        _product = new Product(
            "Contact Lenses",
            "Soft contact lenses",
            29.99m,
            100);
    }

    [Test]
    public void Constructor_ValidData_CreatesProduct()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_product.Name, Is.EqualTo("Contact Lenses"));
            Assert.That(_product.Price, Is.EqualTo(29.99m));
            Assert.That(_product.Stock, Is.EqualTo(100));
            Assert.That(_product.CreatedAtUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(_product.UpdatedAtUtc, Is.Null);
        });
    }

    [TestCase(10, 110)]
    [TestCase(50, 150)]
    [TestCase(1, 101)]
    public void AddStock_ValidQuantity_IncreasesStock(
        int quantity,
        int expectedStock)
    {
        _product.AddStock(quantity);

        Assert.That(_product.Stock, Is.EqualTo(expectedStock));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void AddStock_InvalidQuantity_ThrowsException(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _product.AddStock(quantity));
    }

    [TestCase(10, 90)]
    [TestCase(50, 50)]
    [TestCase(100, 0)]
    public void DecrementStock_ValidQuantity_DecreasesStock(
        int quantity,
        int expectedStock)
    {
        _product.DecrementStock(quantity);

        Assert.That(_product.Stock, Is.EqualTo(expectedStock));
    }

    [Test]
    public void DecrementStock_InsufficientStock_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(
            () => _product.DecrementStock(101));

        Assert.That(_product.Stock, Is.EqualTo(100));
    }

    [Test]
    public void AddStock_Overflow_ThrowsException()
    {
        var product = new Product(
            "Test Product",
            null,
            10m,
            int.MaxValue);

        Assert.Throws<OverflowException>(
            () => product.AddStock(1));
    }

    [Test]
    public void UpdateDetails_ValidData_UpdatesProduct()
    {
        _product.UpdateDetails(
            "Updated Contact Lenses",
            "Updated description",
            39.99m);

        Assert.Multiple(() =>
        {
            Assert.That(_product.Name, Is.EqualTo("Updated Contact Lenses"));
            Assert.That(_product.Description, Is.EqualTo("Updated description"));
            Assert.That(_product.Price, Is.EqualTo(39.99m));
            Assert.That(_product.UpdatedAtUtc, Is.Not.Null);
        });
    }
}
