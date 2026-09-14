using Core;

bool isJson = args.Contains("--json");

EnvironmentReport report = EnvironmentInfo.Collect();

if (isJson)
{
    var options = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
    };
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(report, options));
}
else
{
    Console.WriteLine("CrossApp - інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)  : {report.OsDescription}");
    Console.WriteLine($"Runtime             : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено)     : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)      : {report.ReportedRid}");
    Console.WriteLine($"Примітка збірки    : {report.BuildNote}");
    Console.WriteLine($"Каталог застосунку  : {report.BaseDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область   : (a) Склад (товари, партії, залишки, переміщення)");

if (!Console.IsInputRedirected)
    {
        Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
        try { Console.ReadKey(); } catch { }
    }
}