using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public static class ReportService 
{ 
    
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) => 
        source.Where(p => p.Quantity < threshold) 
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal) 
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)) 
              .ToList(); 

    //  Топ-N  (Дод 2я)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) => 
        source.TopBy(p => p.Quantity, p => p.Sku, n) 
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)) 
              .ToList(); 

    // Дод 2 
    public static IReadOnlyList<GroupSummary> TopGroups(IEnumerable<Product> source, int n) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .TopBy(s => s.TotalQuantity, s => s.Key, n) 
              .ToList();


    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) => 
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase) 
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity))) 
              .OrderByDescending(s => s.TotalQuantity) 
              .ToList(); 

    // Дод 3
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = 
            from product in source
            group product by product.Unit into g
            orderby g.Sum(p => p.Quantity) descending
            select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity));
            
        return query.ToList();
    }

    // Join  Дод 1
    public static IReadOnlyList<MovementLine> ProductMovements(IEnumerable<Product> products, IEnumerable<Movement> movements)
    {
        return products.Join(movements,
            product => product.Id,
            movement => movement.ProductId,
            (product, movement) => new MovementLine(product.Sku, movement.Delta, movement.Operation))
            .ToList();
    }


    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source) 
    { 
        var list = source.ToList(); 
        return list.Count == 0 
            ? (0, 0, 0d, 0) 
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity)); 
    } 

    // =відкладене виконання
    public static IEnumerable<Product> GetLowStockQuery(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold);

    // Дод 4
    public static (long T1, int Counter1, long T2, int Counter2) MeasurePerformance()
    {
        var largeSource = Enumerable.Range(1, 100000)
            .Select(i => Product.Create(Guid.NewGuid().ToString("N")[..8], $"SKU-{i}", $"Товар {i}", "шт", i % 100))
            .ToList();

        // Прогрів JIT
        _ = largeSource.Where(p => p.Quantity > 50).Take(1).ToList();

        var sw = new Stopwatch();

        // 1. БЕЗ ToList 
        int counter1 = 0;
        sw.Start();
        var query1 = largeSource
            .Where(p => { counter1++; return p.Quantity > 50; })
            .Take(1000)
            .ToList();
        sw.Stop();
        long t1 = sw.ElapsedTicks;

        // 2. ToList 
        int counter2 = 0;
        sw.Restart();
        var query2 = largeSource
            .Where(p => { counter2++; return p.Quantity > 50; })
            .ToList()
            .Take(1000)
            .ToList();
        sw.Stop();
        long t2 = sw.ElapsedTicks;

        return (t1, counter1, t2, counter2);
    }
}