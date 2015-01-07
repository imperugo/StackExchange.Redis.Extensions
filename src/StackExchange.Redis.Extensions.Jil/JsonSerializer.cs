using System;
using Jil;

namespace StackExchange.Redis.Extensions.Jil
{
	public class JsonSerializer : ISerializer
	{
		public byte[] Serialize(object item)
		{
			return GetBytes(JSON.Serialize(item));
		}

		public object Deserialize(byte[] bytes)
		{
			return JSON.Deserialize<object>(GetString(bytes));
		}

		public T Deserialize<T>(byte[] bytes) where T : class
		{
			return JSON.Deserialize<T>(GetString(bytes));
		}

		private byte[] GetBytes(string str)
		{
			var bytes = new byte[str.Length*sizeof (char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private string GetString(byte[] bytes)
		{
			var chars = new char[bytes.Length/sizeof (char)];
			Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}
	}
}