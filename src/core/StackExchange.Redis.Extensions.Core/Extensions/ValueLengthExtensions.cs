// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Extensions;

internal static class ValueLengthExtensions
{
    public static IEnumerable<KeyValuePair<string, byte[]>> OfValueInListSize<T>(this IEnumerable<Tuple<string, T>> items, ISerializer serializer, uint maxValueLength)
    {
        using var iterator = items.GetEnumerator();

        while (iterator.MoveNext())
        {
            if (iterator.Current != null)
            {
                yield return new(
                    iterator.Current.Item1,
                    iterator.Current.Item2.SerializeItem(serializer).CheckLength(maxValueLength, iterator.Current.Item1));
            }
        }
    }

    public static byte[] OfValueSize<T>(this T? value, ISerializer serializer, uint maxValueLength, string key)
    {
        return value == null
            ? []
            : serializer.Serialize(value).CheckLength(maxValueLength, key);
    }

    private static byte[] SerializeItem<T>(this T? item, ISerializer serializer)
    {
        return item == null
            ? []
            : serializer.Serialize(item);
    }

    private static byte[] CheckLength(this byte[] byteArray, uint maxValueLength, string paramName)
    {
        if (maxValueLength > default(uint) && byteArray.Length > maxValueLength)
            throw new ArgumentException("value cannot be longer than the MaxValueLength", paramName);

        return byteArray;
    }

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
