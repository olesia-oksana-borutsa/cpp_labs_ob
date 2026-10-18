namespace Core.Reports;

// Зведений DTO для результату з'єднання товарів та переміщень
public sealed record MovementLine(string Sku, int Delta, string Operation);