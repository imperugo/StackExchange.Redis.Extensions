namespace StackExchange.Redis.Extensions.Core.Abstractions
{
	public interface IRedisCacheConnectionPoolManager
    {
        IConnectionMultiplexer GetConnection();
    }
}
