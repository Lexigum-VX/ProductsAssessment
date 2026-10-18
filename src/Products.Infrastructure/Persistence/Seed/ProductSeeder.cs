
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence.Seed;

public static class ProductSeeder
{
    public static void Seed(ProductDbContext context)
    {
        if (context.Products.Any())
            return;

        context.Products.AddRange(CreateProducts());
        context.SaveChanges();
    }

    public static async Task SeedAsync(
        ProductDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        await context.Products.AddRangeAsync(CreateProducts(), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }


    private static Product[] CreateProducts() =>
    [
        new(
        "Contact Lenses",
        "Soft contact lenses for daily vision correction.",
        29.99m,
        150),

    new(
        "Progressive Lenses",
        "Multifocal lenses for near, intermediate and distance vision.",
        249.99m,
        75),

    new(
        "Single Vision Lenses",
        "Lenses designed to correct vision at a single distance.",
        99.99m,
        120),

    new(
        "Bifocal Lenses",
        "Lenses with two distinct optical powers.",
        149.99m,
        60),

    new(
        "Photochromic Lenses",
        "Lenses that adapt to changing light conditions.",
        189.99m,
        90),

    new(
        "Polarized Lenses",
        "Lenses designed to reduce glare from reflected light.",
        129.99m,
        85),

    new(
        "Blue Light Filtering Lenses",
        "Lenses with blue-light filtering technology.",
        119.99m,
        200),

    new(
        "Intraocular Lenses",
        "Artificial lenses used in ophthalmic procedures.",
        399.99m,
        35),

    new(
        "Toric Contact Lenses",
        "Contact lenses designed to correct astigmatism.",
        49.99m,
        110),

    new(
        "Multifocal Contact Lenses",
        "Contact lenses designed for vision at multiple distances.",
        69.99m,
        45)
    ];

}
