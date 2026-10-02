namespace Core.Domain;

public enum OrderStatus
{
    Draft,      // Чернетка
    Confirmed,  // Підтверджено
    Shipped,    // Відправлено
    Cancelled   // Скасовано
}

public sealed class WarehouseOrder
{
    private readonly List<OrderLine> _lines = new List<OrderLine>();

    public string OrderId { get; }
    public OrderStatus Status { get; private set; }


    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    public WarehouseOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("ID замовлення обов'язковий", nameof(orderId));

        OrderId = orderId.Trim();
        Status = OrderStatus.Draft; 
    }

   
   
    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Замовлення {OrderId} вже підтверджене або змінене, рядки додавати не можна.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль.");

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною.");

        _lines.Add(new OrderLine(productId, name, price, quantity));
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true, 
            _ => false 
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException($"Неможливий перехід статусу замовлення {OrderId} із '{Status}' у '{newStatus}'.");
        }

        Status = newStatus;
    }
}