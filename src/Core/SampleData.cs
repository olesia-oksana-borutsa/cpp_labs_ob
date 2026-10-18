using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() => new()
    {
        Product.Create("ART-001", "CR-A-01", "Акрилова фарба 100мл", "шт", 45),
        Product.Create("ART-002", "BR-F-03", "Пензель колонок №2", "шт", 120),
        Product.Create("ART-003", "CV-L-10", "Полотно 30х40 см", "шт", 25),
        Product.Create("ART-004", "MK-W-02", "Бджолиний віск (1кг)", "кг", 15),
        
        // Додані нові записи для Лабораторної №7
        Product.Create("ART-005", "SKB-A5", "Скетчбук А5", "шт", 50),
        Product.Create("ART-006", "SKB-A4", "Скетчбук А4", "шт", 20),
        Product.Create("PEN-001", "PEN-GEL", "Ручка гелева", "шт", 120),
        Product.Create("PEN-002", "PEN-GRF", "Олівець графітний", "шт", 5), // < 10 (Дефіцит для report-low)
        Product.Create("PPR-001", "PPR-A4", "Папір А4 (пачка)", "уп", 15),
        Product.Create("PPR-002", "PPR-A3", "Папір А3 (пачка)", "уп", 8),  // < 10 (Дефіцит для report-low)
        Product.Create("PNT-001", "PNT-ACR", "Фарба акрилова набір", "уп", 35),
        Product.Create("PNT-002", "PNT-OIL", "Фарба олійна", "шт", 12),
        Product.Create("BRS-001", "BRS-SYN", "Пензель синтетика", "шт", 45),
        Product.Create("BRS-002", "BRS-BRS", "Пензель щетина", "шт", 60),
        Product.Create("CVN-001", "CVN-3040", "Полотно 30х40", "шт", 25),
        Product.Create("CVN-002", "CVN-5070", "Полотно 50х70", "шт", 7),   // < 10 (Дефіцит для report-low)
        Product.Create("FLR-001", "FLR-DRW", "Папка для креслення", "шт", 30),
        Product.Create("GLU-001", "GLU-PVA", "Клей ПВА 100мл", "шт", 40),
        Product.Create("GLU-002", "GLU-STK", "Клей-олівець", "шт", 15),
        Product.Create("MSC-001", "MSC-TPE", "Скотч пакувальний", "уп", 55)
    };
}