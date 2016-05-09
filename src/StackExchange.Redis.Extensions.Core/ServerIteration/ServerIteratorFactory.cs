using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.ServerIteration
{
	public class ServerIteratorFactory
	{
		public static IEnumerable<IServer> GetServers(
			ConnectionMultiplexer multiplexer,
			ServerEnumerationStrategy serverEnumerationStrategy = null)
		{
			serverEnumerationStrategy = serverEnumerationStrategy ?? new ServerEnumerationStrategy();

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
