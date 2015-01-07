using System.Collections.Generic;
using System.Net;

namespace StackExchange.Redis.Extensions.Tests.Extensions
{
	internal static class RedisDatabaseExtensions
	{
		public static void FlushDatabase(this IDatabase db)
		{
			var keys = new HashSet<RedisKey>();

			var endPoints = db.Multiplexer.GetEndPoints();

			foreach (EndPoint endpoint in endPoints)
			{
				var dbKeys = db.Multiplexer.GetServer(endpoint).Keys();

				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			foreach (RedisKey key in keys)
			{
				db.KeyDelete(key);
			}
		}
	}
}
