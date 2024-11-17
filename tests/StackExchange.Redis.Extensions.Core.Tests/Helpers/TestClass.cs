// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using MemoryPack;

namespace StackExchange.Redis.Extensions.Core.Tests.Helpers;

#nullable disable

[Serializable]
[DataContract]
[MemoryPackable]
public partial class TestClass<T> : IEquatable<TestClass<T>>
{
    [MemoryPackConstructor]
    public TestClass()
    {
    }

    public TestClass(string key, T value)
    {
        Key = key;
        Value = value;
    }

    [DataMember(Order = 1)]
    public string Key { get; set; }

    [DataMember(Order = 2)]
    public T Value { get; set; }

    public static bool operator ==(TestClass<T> left, TestClass<T> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TestClass<T> left, TestClass<T> right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TestClass<T>);
    }

    public bool Equals(TestClass<T> other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Key, other.Key, StringComparison.Ordinal) && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var keyHash = Key?.GetHashCode(StringComparison.Ordinal) ?? 0;
            var valueHash = EqualityComparer<T>.Default.GetHashCode(Value!);

            return (keyHash * 397) ^ valueHash;
        }
    }
}
