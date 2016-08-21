using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configurations;
using StackExchange.Redis.Extensions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, Action<ConfigurationOptions> configurator)
        {
            var configuration = new ConfigurationOptions();
            configurator(configuration);

            return AddRedis(services, configuration);
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, Func<ConfigurationOptions> configurator)
        {
            return AddRedis(services, configurator());
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, ConfigurationOptions configuration)
        {
            return services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration));
        }

        public static IServiceCollection AddRedisExtensions(this IServiceCollection services, Action<RedisExtensionsConfigurations> configurator)
        {
            var configuration = new RedisExtensionsConfigurations();
            configurator(configuration);

            return services.AddTransient<ICacheClient, StackExchangeRedisCacheClient>();
        }

        public static IServiceCollection AddRedisExtensions(this IServiceCollection services, Func<RedisExtensionsConfigurations> configurator)
        {
            var configuration = configurator();

            return services.AddTransient<ICacheClient, StackExchangeRedisCacheClient>();
        }
    }
}
