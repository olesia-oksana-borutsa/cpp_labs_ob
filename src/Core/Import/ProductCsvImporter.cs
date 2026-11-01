using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<object> Load(string path, Action<string>? onRowSkipped = null)
    {
        var items = new List<object>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;


            string cleanLine = line.TrimStart('\uFEFF', ' ').Trim();


            if (cleanLine.StartsWith("id", StringComparison.OrdinalIgnoreCase) || 
                cleanLine.Contains("sku", StringComparison.OrdinalIgnoreCase) || 
                cleanLine.Contains("category", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(cleanLine))
            {
                case ParseProductOk prod:
                    items.Add(prod.Value);
                    break;
                case ParseWarehouseOk wh:
                    items.Add(wh.Value);
                    break;
                case ParseFailed failed:
                    string errorMsg = $"рядок {number}: {failed.Reason}";
                    errors.Add(errorMsg);
                    
                    
                    onRowSkipped?.Invoke(errorMsg); 
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            
            [var id, var code, var title, var city] when string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title)
                => new ParseFailed("код або назва складу порожні"),
            
            [var id, var code, var title, var city]
                => new ParseWarehouseOk(new WarehouseDto(id, code, title, city)),


            [var id, var sku, var name, var category, var qty] when string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name)
                => new ParseFailed("SKU або назва товару порожні"),
            
            [var id, var sku, var name, var category, var qty] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість товару '{qty}' не є невід'ємним числом"),
            
            [var id, var sku, var name, var category, var qty]
                => new ParseProductOk(new ProductDto(id, sku, name, category, int.Parse(qty, CultureInfo.InvariantCulture))),

            { Length: < 4 } => new ParseFailed($"очікую мінімум 4 колонки, отримав {parts.Length}"),
            _ => new ParseFailed($"невідомий формат рядка або забагато колонок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}