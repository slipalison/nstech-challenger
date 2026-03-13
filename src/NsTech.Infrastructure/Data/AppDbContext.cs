using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;
using NsTech.Infrastructure.Data.Configurations;

namespace NsTech.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
        
        modelBuilder.Entity<Product>().Property(p => p.Version).IsRowVersion(); // otimista 
    }
}
