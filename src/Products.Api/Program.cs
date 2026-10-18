
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("ProductsDb")
    ?? throw new InvalidOperationException(
        "Connection string 'ProductsDb' is not configured.");

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

