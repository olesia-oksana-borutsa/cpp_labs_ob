using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        // 1. ОДРАЗУ перевіряємо токен! Це рятує Тест 5 від падіння з FileNotFoundException
        ct.ThrowIfCancellationRequested(); 

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");
            
        var result = new List<Product>(dtos.Count);
        
        for (var i = 0; i < dtos.Count; i++)
        {
            // 2. Перевірка токена всередині циклу
            ct.ThrowIfCancellationRequested(); 

            // Додаткова бізнес-перевірка: якщо кількість дорівнює 0, кидаємо DomainException
            if (dtos[i].Quantity == 0)
            {
                throw new DomainException($"Запис {i + 1}: кількість не може бути 0.");
            }
            
            try 
            { 
                result.Add(Product.FromDto(dtos[i])); 
            }
            catch (ArgumentException ex)
            { 
                // 3. Загортаємо помилку формату даних у доменний виняток
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); 
            }
        }
        
        return result;
    }
}