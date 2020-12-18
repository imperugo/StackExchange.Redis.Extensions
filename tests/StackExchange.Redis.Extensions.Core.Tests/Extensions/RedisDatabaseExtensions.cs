using System.Collections.Generic;
using System.Net;

namespace StackExchange.Redis.Extensions.Tests.Extensions
{
    internal static class RedisDatabaseExtensions
    {
        public static void FlushDatabase(this IDatabase db)
        {
            foreach (var endpoint in db.Multiplexer.GetEndPoints())
            {
                var server = db.Multiplexer.GetServer(endpoint);

                server.FlushDatabase();
            }
        }
    }
}
