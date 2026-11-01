using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class EnumerableExtensions
{
    
    public static IEnumerable<T> TopBy<T, TKey1, TKey2>(
        this IEnumerable<T> source, 
        Func<T, TKey1> primaryKeySelector, 
        Func<T, TKey2> secondaryKeySelector, 
        int n)
    {
        return source.OrderByDescending(primaryKeySelector)
                     .ThenBy(secondaryKeySelector) 
                     .Take(n);
    }
}