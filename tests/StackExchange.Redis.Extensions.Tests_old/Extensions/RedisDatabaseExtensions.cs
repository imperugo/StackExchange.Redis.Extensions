using System.Collections.Generic;
using System.Net;

namespace StackExchange.Redis.Extensions.Tests.Extensions
{
	internal static class RedisDatabaseExtensions
	{
		public static void FlushDatabase(this IDatabase db)
		{
			var endPoints = db.Multiplexer.GetEndPoints();

			foreach (EndPoint endpoint in endPoints)
			{
				var server = db.Multiplexer.GetServer(endpoint);

				server.FlushDatabase();
			}
		}
	}
}
