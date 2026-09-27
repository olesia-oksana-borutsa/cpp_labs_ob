using Core;
using Core.Dto;
using Core.Domain;
using Core.Import;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("CrossApp - інформація про середовище");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription)  : {report.OsDescription}");
Console.WriteLine($"Runtime             : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Лабораторна робота №4: Доменна модель Складу творчих товарів\n");

//  DTO - Domain
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (File.Exists(path))
{
    ImportResult<object> result = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".json" => ProductJsonImporter.Load(path),
        ".csv" or _ => ProductCsvImporter.Load(path)
    };

    Console.WriteLine($"=== Результати імпорту з файлу ({Path.GetExtension(path).ToUpperInvariant()}) ===");
    foreach (var item in result.Items.Take(3))
    {
        if (item is ProductDto p)
        {
            
            try
            {
                var domainProduct = Product.FromDto(p);
                Console.WriteLine($" Успішно відновлено з DTO: {domainProduct}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Помилка відновлення DTO: {ex.Message}");
            }
        }
    }
    Console.WriteLine(new string('-', 65) + "\n");
}

// 7
Console.WriteLine("=== Сценарій 1: Успішна робота з доменною сутністю ===");
Product product = Product.Create("ART-100", "CR-A-01", "Акрилова фарба (ультрамарин)", "шт", 50);
Console.WriteLine($" Початковий стан: {product}");

product.RegisterArrival(30);
Console.WriteLine($" Після приходу (+30): {product}");

product.Issue(20);
Console.WriteLine($" Після видачі (-20): {product}\n");

Console.WriteLine("=== Сценарій 2: Порушення інваріантів (try/catch) ===");
TryDo("Видача більша за залишок (перевитрата)", () => product.Issue(1000));
TryDo("Створення з порожнім SKU", () => Product.Create("ART-101", "", "Пензель", "шт", 10));
TryDo("Створення з від'ємним початковим залишком", () => Product.Create("ART-102", "SKU-02", "Полотно", "шт", -5));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [!] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        
        Console.WriteLine($"  [OK] {title} -> {ex.GetType().Name}: {ex.Message}");
    }
}

if (!Console.IsInputRedirected)
{
    Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
    try { Console.ReadKey(); } catch { }
}

return 0;