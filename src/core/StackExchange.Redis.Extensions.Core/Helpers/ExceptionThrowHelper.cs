using System;
using System.Diagnostics.CodeAnalysis;

using StackExchange.Redis.Extensions.Core.Extensions;

namespace StackExchange.Redis.Extensions.Core.Helpers;
internal static class ExceptionThrowHelper
{
    public static void ThrowIfSpanEmpty<T>(ReadOnlySpan<T> argument, string paramName)
    {
        if (argument.IsEmpty)
            throw new ArgumentException("The argument cannot be empty.", paramName);
    }

    public static void ThrowIfExistsNullElement<T>(ReadOnlySpan<T> argument, string paramName)
    {
        if (argument.Any(x => x is null))
            ThrowNullElementException(paramName);
    }

    [DoesNotReturn]
    private static void ThrowNullElementException(string? paramName)
    {
        throw new ArgumentException("items cannot contains any null item.", paramName);
    }
}
