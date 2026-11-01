using Core.Dto;
using Core.Domain;

namespace Core.Import;

public static class ProductDomainImporter
{
    // !!! Action<string>
    public static ImportResult<Product> LoadAndValidateDomain(string filePath, Action<string>? onRowSkipped = null)
    {
        
        
        var rawResult = filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) 
            ? ProductJsonImporter.Load(filePath) 
            : ProductCsvImporter.Load(filePath, onRowSkipped);

        var validProducts = new List<Product>();
        var domainErrors = new List<string>(rawResult.Errors);
        
        for (int i = 0; i < rawResult.Items.Count; i++)
        {
            var item = rawResult.Items[i];
            
            if (item is ProductDto dto)
            {
                try
                {
                    var product = Product.FromDto(dto);
                    validProducts.Add(product);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Товар [SKU: {dto.Sku}] не пройшов доменну валідацію: {ex.Message}";
                    domainErrors.Add(errorMsg);
                    
                    //!!!!
                    onRowSkipped?.Invoke(errorMsg); 
                }
            }
        }

        return new ImportResult<Product>(validProducts, domainErrors);
    }
}