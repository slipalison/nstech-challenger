using NsTech.Domain.Enums;

namespace NsTech.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public uint Version { get; private set; }
    public string? IdempotencyKey { get; private set; }

    // Construtor privado para EF Core
    private Order() { }

    public Order(Guid id, string customerId, string currency, List<OrderItem> items, string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Moeda é obrigatória.");
        if (items == null || items.Count == 0) throw new ArgumentException("Pedido deve ter pelo menos um item.");

        Id = id;
        CustomerId = customerId;
        Currency = currency;
        _items.AddRange(items);
        Status = OrderStatus.Placed;
        CreatedAt = DateTime.UtcNow;
        RecalculateTotal();
        IdempotencyKey = idempotencyKey;
    }

    public void RecalculateTotal()
    {
        Total = _items.Sum(x => x.CalculateTotal());
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Confirmed) return; // Idempotente
        
        if (Status != OrderStatus.Placed)
            throw new InvalidOperationException($"Não é possível confirmar um pedido no estado {Status}.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Canceled) return; // Idempotente

        if (Status != OrderStatus.Placed && Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Não é possível cancelar um pedido no estado {Status}.");

        Status = OrderStatus.Canceled;
    }
}
