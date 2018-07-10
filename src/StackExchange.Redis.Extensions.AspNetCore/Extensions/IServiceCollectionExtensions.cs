using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer) where T : ISerializer, new()
		{
			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(connectionMultiplexer, new T()));

			return services;
		}

		
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, string connectionString) where T : ISerializer, new()
		{
			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(new T(), connectionString));

			return services;
		}

		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration) where T : ISerializer, new ()
		{
			services.AddSingleton<ICacheClient>( new StackExchangeRedisCacheClient(new T(), redisConfiguration));

			return services;
		}
	}
}
