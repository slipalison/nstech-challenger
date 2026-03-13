namespace NsTech.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Guid OrderId { get; private set; }

    // Construtor privado para EF Core
    private OrderItem() { }

    public OrderItem(Guid productId, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantidade deve ser maior que zero.");
        if (unitPrice < 0) throw new ArgumentException("Preço unitário não pode ser negativo.");

        Id = Guid.NewGuid();
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal CalculateTotal() => UnitPrice * Quantity;
}
