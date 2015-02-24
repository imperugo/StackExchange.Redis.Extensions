namespace StackExchange.Redis.Extensions.Core.Configuration
{
	public interface IRedisCachingConfiguration
	{
		RedisHostCollection RedisHosts { get; }
		bool AllowAdmin { get; }
		bool Ssl { get; }
		int ConnectTimeout { get; }
		int Db { get; }
	}
}