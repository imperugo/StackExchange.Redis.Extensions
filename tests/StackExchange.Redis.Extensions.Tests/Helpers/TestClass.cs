// //////////////////////////////////////////////////////////////////////////////////////////////////
//  File:			TestClass.cs
//  Website:		http://gaia.is.it/
//  Created:		2015/01/07
//  Last edit:	2015/01/07
// //////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	[Serializable]
	public class TestClass<T>
	{
		public string Key { get; set; }
		public T Value { get; set; }
	}

	[Serializable]
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

		public T Item1 { get; set; }
		public TK Item2 { get; set; }
	}
}