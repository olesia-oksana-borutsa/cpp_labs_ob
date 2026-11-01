using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<object> Load(string path)
    {
        if (!File.Exists(path))
        {
            return new ImportResult<object>([], [$"Файл не знайдено: {path}"]);
        }

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var products = JsonSerializer.Deserialize<List<ProductDto>>(json, options);
            
            if (products == null)
            {
                return new ImportResult<object>([], ["JSON файл порожній або має невірну структуру"]);
            }

            var items = new List<object>();
            var errors = new List<string>();

            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                int itemNumber = i + 1;


                if (string.IsNullOrWhiteSpace(p.Sku) || string.IsNullOrWhiteSpace(p.Name))
                {
                    errors.Add($"об'єкт {itemNumber} (ID: {p.Id ?? "не вказано"}): SKU або назва товару порожні");
                }
                else if (p.Quantity < 0)
                {
                    errors.Add($"об'єкт {itemNumber} (ID: {p.Id}): кількість не може бути від'ємною");
                }
                else
                {
                    items.Add(p);
                }
            }

            return new ImportResult<object>(items, errors);
        }
        catch (JsonException jex)
        {
            return new ImportResult<object>([], [$"Помилка синтаксису JSON: {jex.Message}"]);
        }
        catch (Exception ex)
        {
            return new ImportResult<object>([], [$"Помилка читання JSON файлу: {ex.Message}"]);
        }
    }
}