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
                    iterator.Current.Item2.SerializeItem(serializer)
                        .CheckLength(maxValueLength, iterator.Current.Item1));
            }
        }
    }

    public static byte[] OfValueSize<T>(this T? value, ISerializer serializer, uint maxValueLength, string key)
    {
        return value == null
            ? Array.Empty<byte>()
            : serializer.Serialize(value).CheckLength(maxValueLength, key);
    }

    private static byte[] SerializeItem<T>(this T? item, ISerializer serializer)
    {
        return item == null
            ? Array.Empty<byte>()
            : serializer.Serialize(item);
    }

    private static byte[] CheckLength(this byte[] byteArray, uint maxValueLength, string paramName)
    {
        if (maxValueLength > default(uint) && byteArray.Length > maxValueLength)
            throw new ArgumentException("value cannot be longer than the MaxValueLength", paramName);

        return byteArray;
    }
}
