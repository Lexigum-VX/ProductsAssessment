using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Constants;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint(
                "CK_Products_Id_Range",
                $"[Id] BETWEEN {ProductRules.IdMinValue} AND {ProductRules.IdMaxValue}");

            table.HasCheckConstraint(
                "CK_Products_Stock",
                $"[Stock] >= {ProductRules.MinimumStock}");

            table.HasCheckConstraint(
                "CK_Products_Price",
                $"[Price] >= {ProductRules.MinimumPrice}");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql(
                "NEXT VALUE FOR dbo.ProductIds");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProductRules.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(ProductRules.DescriptionMaxLength);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Stock);
    }
}
