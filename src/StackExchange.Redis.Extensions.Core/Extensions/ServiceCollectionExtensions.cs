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
        public static IServiceCollection AddRedisExtensions(this IServiceCollection services, Action<RedisExtensionsConfigurations> configurator)
        {
            var configuration = new RedisExtensionsConfigurations();
            configurator(configuration);

            if (configuration?.Serializer == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return services.AddSingleton(GetClient(configuration));
        }

        public static IServiceCollection AddRedisExtensions(this IServiceCollection services, Func<RedisExtensionsConfigurations> configurator)
        {
            var configuration = configurator();

            if (configuration?.Serializer == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return services.AddSingleton(GetClient(configuration));
        }

        public static IServiceCollection AddRedisExtensions(this IServiceCollection services, RedisExtensionsConfigurations configuration)
        {
            if (configuration?.Serializer == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return services.AddSingleton(GetClient(configuration));
        }

        private static ICacheClient GetClient(RedisExtensionsConfigurations configuration) =>
            new StackExchangeRedisCacheClient(configuration.Serializer, configuration.ConfigurationOptions);
    }
}
