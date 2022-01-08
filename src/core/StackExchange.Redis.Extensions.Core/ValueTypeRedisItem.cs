// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// The container class for value types
/// </summary>
/// <typeparam name="T"></typeparam>
public class ValueTypeRedisItem<T> where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueTypeRedisItem{T}"/> class.
    /// </summary>
    public ValueTypeRedisItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueTypeRedisItem{T}"/> class.
    /// </summary>
    /// <param name="value">The Value</param>
    public ValueTypeRedisItem(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Return the specified value
    /// </summary>
    public T Value { get; set; }
}
