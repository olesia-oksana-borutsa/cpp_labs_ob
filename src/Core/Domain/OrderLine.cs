namespace Core.Domain;

public sealed record OrderLine
{
    public string ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    public OrderLine(string productId, string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару в рядку обов'язковий", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару в рядку не може бути порожньою", nameof(name));
            
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");
            
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");

        ProductId = productId.Trim();
        Name = name.Trim();
        Price = price;
        Quantity = quantity;
    }
}