using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{

	/// <summary>
	/// 	Adds behavior to System.Linq.
	/// </summary>
	public static class LinqExtensions
	{
		/// <summary>
		/// 	Eaches the specified enumeration.
		/// </summary>
		/// <typeparam name = "T"></typeparam>
		/// <param name = "source">The enumeration.</param>
		/// <param name = "action">The action.</param>
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T item in source)
			{
				action(item);
			}
		}

		/// <summary>
		/// Fors the each asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="body">The body.</param>
		/// <returns></returns>
		public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
		{
			return Task.WhenAll(
				from item in source
				select Task.Run(() => body(item)));
		}
	}
}