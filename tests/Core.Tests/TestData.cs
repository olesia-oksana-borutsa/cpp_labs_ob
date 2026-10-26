using System;
using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    
    public static IEnumerable<Product> Items()
    {
        yield return Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-LOW1", "Товар дефіцит 1", "шт", 5);
        yield return Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-LOW2", "Товар дефіцит 2", "шт", 8);
        yield return Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-MID", "Товар норма", "шт", 50);
        yield return Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-BIG", "Товар багато", "шт", 150);
    }
}