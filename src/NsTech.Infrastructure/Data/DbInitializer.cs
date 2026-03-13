using NsTech.Domain.Entities;

namespace NsTech.Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        // Limpa as tabelas antes do seed para garantir um ambiente limpo em desenvolvimento
        context.OrderItems.RemoveRange(context.OrderItems);
        context.Orders.RemoveRange(context.Orders);
        context.Products.RemoveRange(context.Products);
        context.SaveChanges();

        var products = new List<Product>();
        for (int i = 1; i <= 10; i++)
        {
            products.Add(new Product(
                Guid.Parse($"00000000-0000-0000-0000-0000000000{i:D2}"),
                $"Product {i}",
                10.0m * i,
                100
            ));
        }

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
