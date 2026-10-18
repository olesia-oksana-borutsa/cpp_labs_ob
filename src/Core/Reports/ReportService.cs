using System;
using System.Collections.Generic;
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

    // TopBy Дод 2
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) => 
        source.TopBy(p => p.Quantity, n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)) 
              .ToList(); 

    //TopBy Дод 2
    public static IReadOnlyList<GroupSummary> TopGroups(IEnumerable<Product> source, int n) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .TopBy(s => s.TotalQuantity, n) 
              .ToList();

    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) => 
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase) 
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity))) 
              .OrderByDescending(s => s.TotalQuantity) 
              .ToList(); 

    //Дод 3
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = 
            from product in source
            group product by product.Unit into g
            orderby g.Sum(p => p.Quantity) descending
            select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity));
            
        return query.ToList();
    }

    // Join (Дод 1)
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
}