using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extensions for adding an <see cref="ISerializer"/> implementation to a <see cref="IServiceCollection"/>.
	/// </summary>
	public static class IServiceCollectionExtensions
	{
		/// <summary>
		/// Add <typeparamref name="T"/> serializer to an AspNetCore service collection.
		/// </summary>
		/// <typeparam name="T">type of <see cref="ISerializer"/> to add.</typeparam>
		/// <param name="services">Collection to modify.</param>
		/// <param name="connectionMultiplexer"><see cref="IConnectionMultiplexer"/> to configure Redis Clients.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer)
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<ISerializer, T>();

			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(connectionMultiplexer, new T()));

			return services;
		}

		/// <summary>
		/// Add <typeparamref name="T"/> serializer to an AspNetCore service collection.
		/// </summary>
		/// <typeparam name="T">type of <see cref="ISerializer"/> to add.</typeparam>
		/// <param name="services">Collection to modify.</param>
		/// <param name="connectionString">Connection string for connecting to redis.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, string connectionString)
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
			services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<ISerializer, T>();

			services.AddSingleton<ICacheClient>(new StackExchangeRedisCacheClient(new T(), connectionString));

			return services;
		}

		/// <summary>
		/// Add <typeparamref name="T"/> serializer to an AspNetCore service collection.
		/// </summary>
		/// <typeparam name="T">type of <see cref="ISerializer"/> to add.</typeparam>
		/// <param name="services">Collection to modify.</param>
		/// <param name="redisConfiguration"><see cref="RedisConfiguration"/> to configure Redis Clients.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration) 
			where T : class, ISerializer, new ()
		{
			services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
			services.AddSingleton<ISerializer, T>();
            
            services.AddSingleton<ICacheClient>( new StackExchangeRedisCacheClient(new T(), redisConfiguration));

            services.AddSingleton(redisConfiguration);

            return services;
		}
	}
}
