using System;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
	public interface IRedisCacheConnectionPoolManager : IDisposable
    {
        IConnectionMultiplexer GetConnection();
    }
}
