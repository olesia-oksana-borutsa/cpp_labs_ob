using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class EnumerableExtensions
{
    // Власний метод-розширення TopBy, який можна викликати як .TopBy(...)
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, int> keySelector, int n)
    {
        return source.OrderByDescending(keySelector).Take(n);
    }
}