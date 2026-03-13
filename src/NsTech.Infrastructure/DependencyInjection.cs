using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NsTech.Domain.Interfaces;
using NsTech.Infrastructure.Data;
using NsTech.Infrastructure.Repositories;

namespace NsTech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
