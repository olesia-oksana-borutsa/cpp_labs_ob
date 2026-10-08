using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;
using Core.Events; 

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

/* 5 
Console.WriteLine("=== Лабораторна робота №5: Сервісний шар та сховища ===");
Console.WriteLine($"Підключене сховище: {store.GetType().Name}\n");

Console.WriteLine("--- 1. Список усіх товарів у сховищі ---");
PrintCatalog(service.All());

Console.WriteLine("\n--- 2. Додавання нового товару та прихід ---");
var created5 = service.Add("SKU-999", "Маркери художні (набір)", "наб", 10);
Console.WriteLine($"Створено товар: {created5.Id} | {created5.Sku} | {created5.Name}");

service.Receive(created5.Id, 15);
Console.WriteLine($"Після приходу (+15): {service.Find(created5.Id)}");

Console.WriteLine("\n--- 3. Пошук за параметром Func<Product, bool> (Завдання 2) ---");
Console.WriteLine(" Товари із залишком >= 25:");
var highQuantityProducts = service.Find(p => p.Quantity >= 25);
PrintCatalog(highQuantityProducts);

Console.WriteLine("\n--- 4. Демонстрація сценаріїв відмови (try/catch) ---");
TryDo("Спроба приходу для неіснуючого ID", () => service.Receive("NON-EXISTENT-ID", 10));
TryDo("Спроба видачі більше за наявний залишок", () => service.Issue(created5.Id, 1000));
TryDo("Спроба додати дублікат ID", () => store.Add(Product.Create("ART-001", "CR-A-01", "Фарба-дублікат", "шт", 10)));
================================================================ */




Console.WriteLine("=== Лабораторна робота №6: Події та делегати ===");


var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!); 

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} -> залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;


int changes = 0;
service.Changed += (_, _) => changes++; 


var created = service.Add("ART-101", "Скетчбук для графіки А5", "шт", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");



service.Changed -= OnCatalogChanged;


service.Receive(created.Id, 5); 
Console.WriteLine($"Подій отримано: {changes}"); // +1 лише від лямбди-лічильника



Console.WriteLine("\n--- Спроба помилкової операції (Сценарій відмови) ---");
try
{
    
    service.Receive("NON-EXISTENT", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}


// --- Крок 7: Делегат замість події там, де потрібне значення ---
Console.WriteLine("\n--- Делегат Func (пошук) ---");
var lowStock = service.FindAll(p => p.Quantity < 10);
Console.WriteLine($"Знайдено товарів із залишком < 10: {lowStock.Count}");


// ================================================================

if (!Console.IsInputRedirected)
{
    Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
    try { Console.ReadKey(); } catch { }
}

return 0;

// Допоміжні методи з минулих лаб (просто залишаємо внизу, вони не заважають)
static void PrintCatalog(IEnumerable<Product> products)
{
    foreach (var p in products)
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