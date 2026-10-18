namespace Core.Domain;

// Імітація запису про рух товару для додаткового завдання
public sealed record Movement(string ProductId, int Delta, string Operation);