// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#if NET7_0
using MemoryPack;
#endif

namespace StackExchange.Redis.Extensions.Tests.Helpers;

[Serializable]
[DataContract]
#if NET7_0
[MemoryPackable]
public partial class TestClass<T> : IEquatable<TestClass<T>>
{
    [MemoryPackConstructor]
#else
public class TestClass<T> : IEquatable<TestClass<T>>
{
#endif
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

        return string.Equals(Key, other.Key) && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Key?.GetHashCode() ?? 0) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
        }
    }
}
