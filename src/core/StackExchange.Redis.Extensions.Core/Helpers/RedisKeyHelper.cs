using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StackExchange.Redis.Extensions.Core.Helpers;

internal static class GenericsExtensions
{
    public static void FastIteration<TSource>(this ICollection<TSource>? request, Action<TSource, int> action)
    {
        if (request == null)
            return;

        if (request is TSource[] sourceArray)
        {
            ref var searchSpace = ref MemoryMarshal.GetReference(sourceArray.AsSpan());

            for (var i = 0; i < sourceArray.Length; i++)
            {
                ref var r = ref Unsafe.Add(ref searchSpace, i);

                action.Invoke(r, i);
            }
        }
        else
        {
            var i = 0;
            foreach (var r in request)
                action.Invoke(r, i++);
        }
    }

    public static TResult[] ToFastArray<TSource, TResult>(this TSource[]? source, Func<TSource, TResult> action)
    {
        if (source is not { Length: > 0 })
            return [];

        var result = new TResult[source.Length];
        for (var i = 0; i < source.Length; i++)
            result[i] = action.Invoke(source[i]);

        return result;
    }

    public static TResult[] ToFastArray<TSource, TResult>(this ICollection<TSource>? source, Func<TSource, TResult> action)
    {
        if (source is null)
            return [];

        var srcCnt = source.Count;
        if (srcCnt == 0)
            return [];

        var result = new TResult[srcCnt];
        var i = 0;
        foreach (var item in source)
        {
            result[i] = action.Invoke(item);
            i++;
        }

        return result;
    }
}
