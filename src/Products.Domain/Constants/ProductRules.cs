namespace Products.Domain.Constants;

public static class ProductRules
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public const int IdMinValue = 100000;
    public const int IdMaxValue = 999999;

    public const decimal MinimumPrice = 0m;
    public const int MinimumStock = 0;
    public const int MinimumStockAdjustment = 1;
    public const int RowVersionLength = 8;
}