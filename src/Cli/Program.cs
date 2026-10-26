using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Dto;
using Core.Reports;
using Core.Import;


// Ініціалізація сховища та сервісу

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);


var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "sample.json");


//  8

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => 
{ 
    e.Cancel = true; 
    cts.Cancel(); 
};

//перевірка таймауту
// cts.CancelAfter(TimeSpan.FromSeconds(2)); 

Console.WriteLine("Асинхронний імпорт (Ctrl+C для скасування) ");
try 
{ 
    var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, cts.Token); 
    Console.WriteLine($"Імпортовано записів: {added}\n"); 
} 
catch (OperationCanceledException) 
{ 
    Console.WriteLine("Імпорт скасовано.\n"); 
} 
catch (DomainException ex)     
{ 
    Console.WriteLine($"Дані відхилено: {ex.Message}\n"); 
} 
catch (FileNotFoundException)     
{ 
    Console.WriteLine($"Файл не знайдено: {dataPath}\n"); 
}


/*/
var items = service.All(); 
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top"; 

if (command == "report-top" && args.Length == 0)
{
    Console.WriteLine("=== Демонстрація відкладеного виконання ===");
    var sourceList = service.All().ToList(); 
    var query = ReportService.GetLowStockQuery(sourceList, 10);

    Console.WriteLine($"1. Дефіцитних до змін: {query.Count()}"); 
    sourceList.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3)); 
    Console.WriteLine($"2. Дефіцитних після змін: {query.Count()}"); 

    var snapshot = query.ToList(); 
    sourceList.Clear(); 
    Console.WriteLine($"3. У знімку лишилось: {snapshot.Count}\n"); 
}

switch (command)
{ 
    case "report-top":    
        Console.WriteLine("Звіти (LINQ) - Топ-5:");
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
        
    case "report-perf":
        Console.WriteLine("Звіти (LINQ) - Тест швидкодії...");
        var perfResult = ReportService.MeasurePerformance();
        Console.WriteLine($"  Без ToList: {perfResult.T1} тіків");
        Console.WriteLine($"  З ToList:   {perfResult.T2} тіків");
        break;
}

return 0;

static void Print(IReadOnlyList<StockLine> lines) 
{ 
    foreach (var l in lines) 
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}"); 
}/*/
