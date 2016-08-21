// //////////////////////////////////////////////////////////////////////////////////////////////////
//  File:			TestClass.cs
//  Website:		http://gaia.is.it/
//  Created:		2015/01/07
//  Last edit:	2015/01/07
// //////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	[Serializable]
    [DataContract]
	public class TestClass<T> : IEquatable<TestClass<T>>
	{
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

	    public override bool Equals(object obj)
	    {
	        return Equals(obj as TestClass<T>);
	    }

	    public bool Equals(TestClass<T> other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return string.Equals(Key, other.Key) && EqualityComparer<T>.Default.Equals(Value, other.Value);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return ((Key?.GetHashCode() ?? 0)*397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
	        }
	    }

	    public static bool operator ==(TestClass<T> left, TestClass<T> right)
	    {
	        return Equals(left, right);
	    }

	    public static bool operator !=(TestClass<T> left, TestClass<T> right)
	    {
	        return !Equals(left, right);
	    }
	}

    [Serializable]
    [DataContract]
	public class ComplexClassForTest<T,TK> : IEquatable<ComplexClassForTest<T, TK>>
    {
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ComplexClassForTest<T, TK>);
        }

        public bool Equals(ComplexClassForTest<T, TK> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Item1, other.Item1) && EqualityComparer<TK>.Default.Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Item1)*397) ^ EqualityComparer<TK>.Default.GetHashCode(Item2);
            }
        }

        public static bool operator ==(ComplexClassForTest<T, TK> left, ComplexClassForTest<T, TK> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComplexClassForTest<T, TK> left, ComplexClassForTest<T, TK> right)
        {
            return !Equals(left, right);
        }
	}
}