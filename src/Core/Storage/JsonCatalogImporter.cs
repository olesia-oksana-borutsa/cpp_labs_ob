using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    // дод 1: Потоковий імпорт через IAsyncEnumerable
    public async IAsyncEnumerable<Product> LoadAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); 

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        // дод 1: DeserializeAsyncEnumerable 
        var dtos = JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(stream, cancellationToken: ct);
        
        int index = 0;
        
        // дод 1: await foreach
        await foreach (var dto in dtos.WithCancellation(ct))
        {
            if (dto is null) continue;
            
            if (dto.Quantity == 0)
                throw new DomainException($"Запис {index + 1}: кількість не може бути 0.");
            
            Product product;
            try 
            { 
                product = Product.FromDto(dto); 
            }
            catch (ArgumentException ex)
            { 
                throw new DomainException($"Запис {index + 1}: {ex.Message}", ex); 
            }
            
            yield return product; 
            index++;
        }
    }
}