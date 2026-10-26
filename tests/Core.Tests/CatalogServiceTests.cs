using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogServiceTests
{
    //1: Перевірка інваріанта (відмова і незмінний стан при від'ємній кількості)
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", "м", 10);


        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));


        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    // 2: Перевірка підняття події рівно один раз
    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e); 


        service.Add("SKU-2", "Гвинт", "шт", 7);


        Assert.Single(events); 
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind); 
    }

    //  3: Перевірка звіту на різних наборах даних 
    [Theory]
    [InlineData(10, 2)] 
    [InlineData(1, 0)]  
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var result = ReportService.LowStock(TestData.Items(), threshold);
        Assert.Equal(expected, result.Count);
    }

    //  4: Перевірка стабільності сортування (Топ-N)
    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        
        var expected = new[] { "SKU-BIG", "SKU-MID" };
        var actual = ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku).ToArray();
        
        Assert.Equal(expected, actual);
    }

    // 5: Перевірка скасування (OperationCanceledException)
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); 
        var service = new CatalogService(new InMemoryCatalogStore());


        
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    //  6: Перевірка доменного винятку на "поганому" файлі
    [Fact]
    public async Task ImportAsync_WithInvalidData_ThrowsDomainException()
    {
        
        var tempFile = Path.GetTempFileName();

        //  товару від'ємна кількість

        var invalidJson = "[{\"Id\":\"invalid-1\",\"Sku\":\"TEST\",\"Name\":\"A\",\"Unit\":\"шт\",\"Quantity\":-10}]";
        
        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJson);
            var service = new CatalogService(new InMemoryCatalogStore());


            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile, CancellationToken.None));
            
            
            Assert.Contains("Запис 1", ex.Message);
        }
        finally
        {
            
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}