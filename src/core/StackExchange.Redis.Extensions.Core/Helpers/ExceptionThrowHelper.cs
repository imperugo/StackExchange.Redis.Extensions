// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace StackExchange.Redis.Extensions.Core.Helpers;

internal static class ExceptionThrowHelper
{
    public static void ThrowIfExistsNullElement<T>(ReadOnlySpan<T> argument, string paramName)
    {
        foreach (var item in argument)
        {
            if (item is null)
                ThrowNullElementException(paramName);
        }
    }

    [DoesNotReturn]
    private static void ThrowNullElementException(string? paramName)
    {
        throw new ArgumentException("items cannot contains any null item.", paramName);
    }
}
