// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using MemoryPack;

#nullable disable

namespace StackExchange.Redis.Extensions.Core.Tests.Helpers;

[Serializable]
[DataContract]
[MemoryPackable]
public partial class ComplexClassForTest<T, TK> : IEquatable<ComplexClassForTest<T, TK>>
{
    [MemoryPackConstructor]
    public ComplexClassForTest()
    {
    }

    public ComplexClassForTest(T item1, TK item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    [DataMember(Order = 1)]
    public T Item1 { get; set; }

    [DataMember(Order = 2)]
    public TK Item2 { get; set; }

    public static bool operator ==(ComplexClassForTest<T, TK> left, ComplexClassForTest<T, TK> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ComplexClassForTest<T, TK> left, ComplexClassForTest<T, TK> right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ComplexClassForTest<T, TK>);
    }

    public bool Equals(ComplexClassForTest<T, TK> other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return EqualityComparer<T>.Default.Equals(Item1, other.Item1) && EqualityComparer<TK>.Default.Equals(Item2, other.Item2);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var item1Hash = Item1 != null ? EqualityComparer<T>.Default.GetHashCode(Item1) : 0;
            var item2Hash = Item2 != null ? EqualityComparer<TK>.Default.GetHashCode(Item2) : 0;

            return (item1Hash * 397) ^ item2Hash;
        }
    }
}
