using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.ServerIteration
{
    /// <summary>
    /// The factory that allows you to enumerate all Redis servers.
    /// </summary>
	public class ServerIteratorFactory
    {
        /// <summary>
        /// Rerturn all Redis servers
        /// </summary>
        /// <param name="multiplexer">The redis connection.</param>
        /// <param name="serverEnumerationStrategy">The iterate strategy.</param>
        /// <returns></returns>
        public static IEnumerable<IServer> GetServers(
            IConnectionMultiplexer multiplexer,
            ServerEnumerationStrategy serverEnumerationStrategy)
        {
            switch (serverEnumerationStrategy.Mode)
            {
                case ServerEnumerationStrategy.ModeOptions.All:
                    var serversAll = new ServerEnumerable(multiplexer,
                        serverEnumerationStrategy.TargetRole,
                        serverEnumerationStrategy.UnreachableServerAction);
                    return serversAll;

                case ServerEnumerationStrategy.ModeOptions.Single:
                    var serversSingle = new ServerEnumerable(multiplexer,
                        serverEnumerationStrategy.TargetRole,
                        serverEnumerationStrategy.UnreachableServerAction);
                    return serversSingle.Take(1);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
