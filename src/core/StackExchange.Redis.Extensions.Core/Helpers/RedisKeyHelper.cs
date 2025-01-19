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

    public static TResult[] ToFastArray<TSource, TResult>(this ICollection<TSource>? request, Func<TSource, TResult> action)
    {
        if (request == null)
            return [];

        var result = new TResult[request.Count];

        if (request is TSource[] sourceArray)
        {
            ref var searchSpace = ref MemoryMarshal.GetReference(sourceArray.AsSpan());

            for (var i = 0; i < sourceArray.Length; i++)
            {
                ref var r = ref Unsafe.Add(ref searchSpace, i);

                result[i] = action.Invoke(r);
            }
        }
        /*

         This could be helpful when we drop old frameworks

        else if (request is List<TSource> sourceList)
        {
            var span = CollectionsMarshal.AsSpan(sourceList);
            ref var searchSpace = ref MemoryMarshal.GetReference(span);

            for (var i = 0; i < span.Length; i++)
            {
                ref var r = ref Unsafe.Add(ref searchSpace, i);

                result[i] =  action.Invoke(r);
            }
        }
        */
        else
        {
            var i = 0;
            foreach (var r in request)
                result[i++] = action.Invoke(r);
        }

        return result;
    }
}
