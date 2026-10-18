using Products.Domain.Constants;

namespace Products.Domain.Entities;

public sealed class Product
{
    private Product() { } // EF Core

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public Product(
        string name,
        string? description,
        decimal price,
        int initialStock)
    {
        UpdateDetails(name, description, price);

        if (initialStock < ProductRules.MinimumStock)
            throw new ArgumentOutOfRangeException(
                nameof(initialStock),
                DomainErrorMessages.CannotBeNegative(nameof(initialStock)));

        Stock = initialStock;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = null;
    }

    public void UpdateDetails(
        string name,
        string? description,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                DomainErrorMessages.Required(nameof(Name)),
                nameof(name));

        name = name.Trim();

        if (name.Length > ProductRules.NameMaxLength)
            throw new ArgumentException(
                DomainErrorMessages.MaxLength(nameof(Name), ProductRules.NameMaxLength),
                nameof(name));

        if (description?.Length > ProductRules.DescriptionMaxLength)
            throw new ArgumentException(
                DomainErrorMessages.MaxLength(nameof(Description), ProductRules.DescriptionMaxLength),
                nameof(description));

        if (price < ProductRules.MinimumPrice)
            throw new ArgumentOutOfRangeException(
                nameof(price),
                DomainErrorMessages.CannotBeNegative(nameof(price)));

        if (price > ProductRules.MaximumPrice)
            throw new ArgumentOutOfRangeException(
                nameof(price),
                DomainErrorMessages.MaximumValue(
                    nameof(Price),
                    ProductRules.MaximumPrice));

        if (!ProductRules.HasValidPricePrecision(price))
            throw new ArgumentException(
                DomainErrorMessages.MaximumDecimalPlaces(
                    nameof(Price),
                    ProductRules.PriceDecimalPlaces),
                nameof(price));

        Name = name;
        Description = description;
        Price = price;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity < ProductRules.MinimumStockAdjustment)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                DomainErrorMessages.MustBePositive(nameof(quantity)));

        Stock = checked(Stock + quantity);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void DecrementStock(int quantity)
    {
        if (quantity < ProductRules.MinimumStockAdjustment)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                DomainErrorMessages.MustBePositive(nameof(quantity)));

        if (Stock < quantity)
            throw new InvalidOperationException(
                DomainErrorMessages.InsufficientStock);

        Stock -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
