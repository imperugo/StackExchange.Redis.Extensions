using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer)
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
			services.AddSingleton<ISerializer, T>();

			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(connectionMultiplexer, new T()));

			return services;
		}

		
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, string connectionString)
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
			services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
			services.AddSingleton<ISerializer, T>();

			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(new T(), connectionString));

			return services;
		}

		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration) 
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
			services.AddSingleton<ISerializer, T>();

			services.AddSingleton<ICacheClient>( new StackExchangeRedisCacheClient(new T(), redisConfiguration));

			return services;
		}
	}
}
