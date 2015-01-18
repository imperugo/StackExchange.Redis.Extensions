using System;
using Jil;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Jil
{
	public class JsonSerializer : ISerializer
	{
		public string Serialize(object item)
		{
			return JSON.Serialize(item);
		}

		public object Deserialize(string serializedObject)
		{
			return JSON.Deserialize(serializedObject, typeof (object));
		}

		public T Deserialize<T>(string serializedObject) where T : class
		{
			return JSON.Deserialize<T>(serializedObject);
		}
	}
}