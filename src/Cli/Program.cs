using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;

/*

// 1.  DTO та Domain 
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (File.Exists(path))
{
    ImportResult<object> result = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".json" => ProductJsonImporter.Load(path),
        ".csv" or _ => ProductCsvImporter.Load(path)
    };

    Console.WriteLine($"=== Результати імпорту з файлу ({Path.GetExtension(path).ToUpperInvariant()}) ===");
    foreach (var item in result.Items.Take(10))
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
Console.WriteLine(new string('-', 65) + "\n");


Console.WriteLine("Додаткове завдання: Доменні сервіси, імпорт та статуси");

// 1: Доменний імпорт з обробкою інваріантів («дані + помилки»)
var domainImportResult = ProductDomainImporter.LoadAndValidateDomain(path);
Console.WriteLine($" 1. Успішно створено доменних сутностей: {domainImportResult.Items.Count}");
foreach (var validProduct in domainImportResult.Items.Take(20)) 
{
    Console.WriteLine($"    [Успіх] {validProduct}");
}

Console.WriteLine($"   Перехоплено доменних помилок/невалідних даних: {domainImportResult.Errors.Count}");
foreach (var error in domainImportResult.Errors) 
{
    Console.WriteLine($"    [Помилка інваріанту] {error}");
}

// 2: Інваріант двох сутностей у сервісі (Склад і Товари)
var warehouseDto = new WarehouseDto("WH-01", "LIV-01", "Головний склад", "Львів");
TryDo("2. Перевищення місткості складу (інваріант двох сутностей)", 
    () => WarehouseService.ValidateWarehouseCapacity(warehouseDto, currentTotalItems: 950, incomingQuantity: 100, maxCapacity: 1000));


var order = new WarehouseOrder("ORD-2026-01");
Console.WriteLine($" 3. Початковий статус замовлення: {order.Status}");

order.AddLine("ART-100", "Акрилова фарба", 150.0m, 2);
order.AddLine("ART-101", "Пензель", 45.0m, 5);

Console.WriteLine($"    Кількість позицій у замовленні (Lines.Count): {order.Lines.Count}");
Console.WriteLine($"    Загальна сума замовлення: {order.Total} грн");
foreach (var line in order.Lines)
{
    Console.WriteLine($"     - Позиція: {line.Name} ({line.Quantity} шт. по {line.Price} грн)");
}

order.ChangeStatus(OrderStatus.Confirmed);
Console.WriteLine($"   Після зміни стану: {order.Status}");

TryDo(" Заборонений перехід статусу (з Confirmed назад у Draft)", () => order.ChangeStatus(OrderStatus.Draft));
*/



bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");


ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine("Лабораторна робота №5: Сервісний шар та сховища");
Console.WriteLine($"Обране сховище: {store.GetType().Name}\n");

Console.WriteLine(" 1. Список товарів у сховищі ");
PrintCatalog(service);

Console.WriteLine("\n2. Додавання нового товару та прихід");
var created = service.Add("SKU-999", "Маркери художні (набір)", "наб", 10);
Console.WriteLine($"Створено товар: {created.Id} | {created.Sku} | {created.Name}");

service.Receive(created.Id, 15);
Console.WriteLine($"Після приходу (+15): {service.Find(created.Id)}");

Console.WriteLine("\n 3. Список товарів після оновлення ");
PrintCatalog(service);

Console.WriteLine("\n 4. Демонстрація сценаріїв відмови (try/catch) ");
TryDo("Спроба приходу для неіснуючого ID", () => service.Receive("NON-EXISTENT-ID", 10));
TryDo("Спроба видачі більше за наявний залишок", () => service.Issue(created.Id, 1000));

if (!Console.IsInputRedirected)
{
    Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
    try { Console.ReadKey(); } catch { }
}

return 0;

static void PrintCatalog(CatalogService service)
{
    foreach (var p in service.All())
    {
        Console.WriteLine($" [{p.Id}] {p.Sku,-10} {p.Name,-25} {p.Quantity,4} {p.Unit}");
    }
}

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