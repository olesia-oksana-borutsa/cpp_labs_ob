using System.Diagnostics;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Import;
using Core.Dto;
using Core.Reports;


ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);



var items = service.All(); 
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top"; 


if (command == "report-top" && args.Length == 0)
{
    Console.WriteLine("=== Лабораторна робота №7: Демонстрація відкладеного виконання ===\n");

    var sourceList = service.All().ToList(); 
    var query = sourceList.Where(p => p.Quantity < 10);

    Console.WriteLine($"1. Дефіцитних до змін у джерелі: {query.Count()}"); 
    foreach (var p in query) 
        Console.WriteLine($"   - [{p.Sku}] {p.Name,-20} (Залишок: {p.Quantity} {p.Unit})");

    Console.WriteLine("\n  НОВИЙ ТОВАР: 'Скоба' (Залишок: 3)");
    sourceList.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3)); 

    Console.WriteLine($"\n2. Дефіцитних після змін : {query.Count()}"); 
    foreach (var p in query) 
        Console.WriteLine($"   - [{p.Sku}] {p.Name,-20} (Залишок: {p.Quantity} {p.Unit})");


    var snapshot = query.ToList(); 
    sourceList.Clear(); 

    Console.WriteLine($"\n3. У знімку  лишилось після очищення: {snapshot.Count}"); 
    foreach (var p in snapshot) 
        Console.WriteLine($"   - [{p.Sku}] {p.Name,-20} (Залишок: {p.Quantity} {p.Unit})");

    Console.WriteLine("\n===================================================================\n");
}



switch (command)
{ 
    case "report-top":    
        Console.WriteLine("Звіти (LINQ)");
        Print(ReportService.TopByQuantity(items, 5)); 
        break; 

    case "report-low":    
        Console.WriteLine("Звіти (LINQ) - Дефіцит:");
        Print(ReportService.LowStock(items, 10)); 
        break; 

    case "report-groups": 
        Console.WriteLine("Звіти (LINQ) - Групування:");
        foreach (var g in ReportService.ByUnit(items)) 
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,6}"); 
        break; 

    case "report-sum": 
        Console.WriteLine("Звіти (LINQ) - Загальна статистика:");
        var t = ReportService.Totals(items); 
        Console.WriteLine($"  позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}"); 
        break; 

    // Дод завдання 1
    case "report-join":
        Console.WriteLine("Звіти (LINQ) - З'єднання колекцій:");
        var pList = items.Take(5).ToList(); 
        var movements = new List<Movement> 
        {
            new Movement(pList[0].Id, 10, "Прибуток"),
            new Movement(pList[0].Id, -2, "Списання"),
            new Movement(pList[1].Id, 5,  "Прибуток"),
            new Movement(pList[2].Id, 50, "Прибуток"),
            new Movement(pList[2].Id, -15, "Видача")
        };
        var joined = ReportService.ProductMovements(pList, movements);
        foreach (var m in joined)
            Console.WriteLine($"  [{m.Sku,-8}] Операція: {m.Operation,-10} Зміна: {m.Delta,4:+0;-0}");
        break;


   // Дод завдання 4 
    case "report-perf":
        Console.WriteLine("Звіти (LINQ) - Тест швидкодії:");
        Console.WriteLine("Генерація 100 000 записів у пам'яті...");
        var largeSource = Enumerable.Range(1, 100000)
            .Select(i => Product.Create(Guid.NewGuid().ToString("N")[..8], $"SKU-{i}", $"Товар {i}", "шт", i % 100))
            .ToList();

        // Прогрів JIT 
        _ = largeSource.Where(p => p.Quantity > 50).Take(1).ToList();

        var sw = new Stopwatch();

        //= БЕЗ ToList
        int counter1 = 0;
        sw.Start();
        var query1 = largeSource
            .Where(p => { counter1++; return p.Quantity > 50; })
            .Take(1000) 
            .ToList();
        sw.Stop();
        long t1 = sw.ElapsedTicks; 

        //  З проміжним ToList 
        int counter2 = 0;
        sw.Restart();
        var query2 = largeSource
            .Where(p => { counter2++; return p.Quantity > 50; })
            .ToList()   
            .Take(1000)
            .ToList();
        sw.Stop();
        long t2 = sw.ElapsedTicks;

        Console.WriteLine("\n[Без проміжного ToList()]");
        Console.WriteLine($"  Витрачено часу : {t1} тіків процесора");
        Console.WriteLine($"  Викликів Where : {counter1} разів (LINQ зупинився, як тільки знайшов 1000 елементів)");

        Console.WriteLine("\n[З проміжним ToList()]");
        Console.WriteLine($"  Витрачено часу : {t2} тіків процесора (значно повільніше)");
        Console.WriteLine($"  Викликів Where : {counter2} разів (ToList змусив перебрати всі 100 000 елементів)");
        break;
}

return 0;


static void Print(IReadOnlyList<StockLine> lines) 
{ 
    foreach (var l in lines) 
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}"); 
}
