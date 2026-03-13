namespace NsTech.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int AvailableQuantity { get; private set; }
    public uint Version { get; private set; }

    // Construtor privado para EF Core
    private Product() { }

    public Product(Guid id, string name, decimal unitPrice, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome não pode ser vazio.");
        if (unitPrice <= 0) throw new ArgumentException("Preço deve ser maior que zero.");
        if (availableQuantity < 0) throw new ArgumentException("Quantidade não pode ser negativa.");

        Id = id;
        Name = name;
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome não pode ser vazio.");
        Name = name;
    }

    public void UpdatePrice(decimal unitPrice)
    {
        if (unitPrice <= 0) throw new ArgumentException("Preço deve ser maior que zero.");
        UnitPrice = unitPrice;
    }

    public void UpdateStock(int availableQuantity)
    {
        if (availableQuantity < 0) throw new ArgumentException("Quantidade não pode ser negativa.");
        AvailableQuantity = availableQuantity;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantidade de reserva deve ser maior que zero.");
        if (AvailableQuantity < quantity) throw new InvalidOperationException("Estoque insuficiente.");
        
        AvailableQuantity -= quantity;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantidade de liberação deve ser maior que zero.");
        
        AvailableQuantity += quantity;
    }
}
