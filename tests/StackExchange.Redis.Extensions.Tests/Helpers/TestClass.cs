// //////////////////////////////////////////////////////////////////////////////////////////////////
//  File:			TestClass.cs
//  Website:		http://gaia.is.it/
//  Created:		2015/01/07
//  Last edit:	2015/01/07
// //////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	[Serializable]
    [DataContract]
	public class TestClass<T>
	{
        [DataMember(Order = 1)]
		public string Key { get; set; }

        [DataMember(Order = 2)]
		public T Value { get; set; }
	}

    [Serializable]
    [DataContract]
	public class ComplexClassForTest<T,TK>
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
	}
}