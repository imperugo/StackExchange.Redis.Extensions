using System;
using StackExchange.Redis.Extensions.Core.Configurations;

namespace StackExchange.Redis.Extensions.Protobuf
{
    public static class ServiceCollectionExtensions
    {
        public static RedisExtensionsConfigurations WithProtobuf(this RedisExtensionsConfigurations redisExtensionsConfigurations)
        {
            redisExtensionsConfigurations.Serializer = new ProtobufSerializer();

            return redisExtensionsConfigurations;
        }
    }
}
