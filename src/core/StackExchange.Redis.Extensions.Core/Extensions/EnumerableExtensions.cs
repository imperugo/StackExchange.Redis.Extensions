using System;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Extensions;

internal static class EnumerableExtensions
{
    public static TSource MinBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> selector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        using var sourceIterator = source.GetEnumerator();

        if (!sourceIterator.MoveNext())
            throw new InvalidOperationException("Sequence contains no elements");

        var min = sourceIterator.Current;
        var minKey = selector(min);

        while (sourceIterator.MoveNext())
        {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);
            if (comparer.Compare(candidateProjected, minKey) < 0)
            {
                min = candidate;
                minKey = candidateProjected;
            }
        }

        return min;
    }
}
