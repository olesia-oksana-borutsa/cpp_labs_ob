using Core;
using Core.Dto;
using Core.Import;


EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("CrossApp - інформація про середовище");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription)  : {report.OsDescription}");
Console.WriteLine($"Runtime             : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено)     : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET)      : {report.ReportedRid}");
Console.WriteLine($"Примітка збірки     : {report.BuildNote}");
Console.WriteLine($"Каталог застосунку  : {report.BaseDirectory}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область   : (a) Склад творчих товарів\n");


string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Помилка: файл не знайдено за шляхом -> {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine("=== Результати імпорту складу творчих товарів ===");
Console.WriteLine($"Успішно завантажено записів: {result.Items.Count}");
Console.WriteLine(new string('-', 65));

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-8} {p.Sku,-10} {p.Name,-30} {p.Quantity,4} шт.");
}

Console.WriteLine(new string('-', 65));

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено пошкоджених рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
}

if (!Console.IsInputRedirected)
{
    Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
    try { Console.ReadKey(); } catch { }
}

return 0;