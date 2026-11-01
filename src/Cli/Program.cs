using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Configuration;
using Core.Services;
using Core.Storage;
using Core.Abstractions;
using Microsoft.Extensions.Configuration;


var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") 
              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
              ?? "Development";


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();


var options = config.Get<AppOptions>() ?? new AppOptions();


var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storageOptions = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacyDataPath) ? options.Storage.DataPath : legacyDataPath,
    FileName = options.Storage.FileName
};


var dataFullPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);


ICatalogStore store = new FileCatalogStore(dataFullPath);
var service = new CatalogService(store);


var apiKey = config["Integration:ApiKey"];
Console.WriteLine($"Файл даних: {dataFullPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");


if (args.Length > 0 && args[0] == "export")
{
    var count = store.List().Count;
    Console.WriteLine($"Експортовано {count} записів.");
    return 0;
}


var samplePath = Path.Combine(AppContext.BaseDirectory, "data", "sample.json");
if (File.Exists(samplePath))
{
    using var userCts = new CancellationTokenSource();
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

    int totalItems = 21;
    var progress = new Progress<int>(processed => 
    {
        int percent = (int)((double)processed / totalItems * 100);
        Console.Write($"\rПрогрес імпорту: {percent}% ({processed}/{totalItems})");
    });

    try 
    {
        var added = await service.ImportAsync(new JsonCatalogImporter(), samplePath, progress, linkedCts.Token);
        Console.WriteLine($"\nІмпортовано нових записів: {added}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\nІмпорт скасовано (таймаут або Ctrl+C).");
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"\nДані відхилено: {ex.Message}");
    }
}

return 0;