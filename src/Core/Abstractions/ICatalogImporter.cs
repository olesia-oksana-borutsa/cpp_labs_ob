using System.Collections.Generic;
using System.Threading;
using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    
    IAsyncEnumerable<Product> LoadAsync(string path, CancellationToken ct = default);
}