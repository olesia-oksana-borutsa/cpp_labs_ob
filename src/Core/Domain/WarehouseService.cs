using Core.Dto;

namespace Core.Domain;

public static class WarehouseService
{
   
    public static void ValidateWarehouseCapacity(WarehouseDto warehouse, int currentTotalItems, int incomingQuantity, int maxCapacity)
    {
        if (currentTotalItems + incomingQuantity > maxCapacity)
        {
            throw new InvalidOperationException(
                $"Склад '{warehouse.Title}' ({warehouse.City}) переповнено! " +
                $"Місткість: {maxCapacity}, спроба розмістити: {currentTotalItems + incomingQuantity}.");
        }
    }
}