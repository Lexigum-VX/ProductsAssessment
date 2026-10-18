using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Application.Services;

namespace Products.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
