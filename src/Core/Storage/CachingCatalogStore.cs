using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{
    private readonly ICatalogStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private Dictionary<string, Product>? _cache;

    private Dictionary<string, Product> GetOrUpdateCache()
    {
        if (_cache == null)
        {
            _cache = _inner.List().ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        }
        return _cache;
    }

    public IReadOnlyList<Product> List() => GetOrUpdateCache().Values.ToList();

    public Product? GetById(string id) => GetOrUpdateCache().GetValueOrDefault(id);

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate) 
        => GetOrUpdateCache().Values.Where(predicate).ToList();

    public void Add(Product item)
    {
        _inner.Add(item);
        GetOrUpdateCache()[item.Id] = item;
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        GetOrUpdateCache()[item.Id] = item;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed)
        {
            GetOrUpdateCache().Remove(id);
        }
        return removed;
    }
}