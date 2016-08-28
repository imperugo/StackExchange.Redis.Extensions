using System;
using StackExchange.Redis.Extensions.Core.Configurations;

namespace StackExchange.Redis.Extensions.Jil
{
    public static class ServiceCollectionExtensions
    {
        public static RedisExtensionsConfigurations WithMsgPack(this RedisExtensionsConfigurations configurator, Options options)
        {
            configurator.Serializer = new JilSerializer(;

            return configurator;
        }
    }
}
