using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        // базове сховище
        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());


        if (useCache)
        {
            store = new CachingCatalogStore(store);
        }

        return store;
    }
}