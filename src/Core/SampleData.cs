using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() => new()
    {
        Product.Create("ART-001", "CR-A-01", "Акрилова фарба 100мл", "шт", 45),
        Product.Create("ART-002", "BR-F-03", "Пензель колонок №2", "шт", 120),
        Product.Create("ART-003", "CV-L-10", "Полотно 30х40 см", "шт", 25),
        Product.Create("ART-004", "MK-W-02", "Бджолиний віск (1кг)", "кг", 15)
    };
}