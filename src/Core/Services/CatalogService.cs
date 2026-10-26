using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store, IEnumerable<INotificationSink>? sinks = null)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly IEnumerable<INotificationSink> _sinks = sinks ?? Array.Empty<INotificationSink>();

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(Product p, CatalogChangeKind kind, int delta)
    {
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity);
        
        Changed?.Invoke(this, args);
        
        foreach (var sink in _sinks)
        {
            sink.Notify(args);
        }
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) => 
        _store.List().Where(predicate).ToList();

    //  Асинхронний імпорт
    
    public async Task<int> ImportAsync(ICatalogImporter importer, string path, CancellationToken ct = default)
    {
        var items = await importer.LoadAsync(path, ct);
        var added = 0;

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested(); 
            await Task.Delay(200, ct);
            
            if (_store.GetById(item.Id) is not null) 
                continue;   
            
            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;
        }

        return added;
    }
}