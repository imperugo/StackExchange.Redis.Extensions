using System;
using StackExchange.Redis.Extensions.Core.Configurations;
using MsgPack.Serialization;

namespace StackExchange.Redis.Extensions.MsgPack
{
    public static class ServiceCollectionExtensions
    {
        public static RedisExtensionsConfigurations WithMsgPack(this RedisExtensionsConfigurations redisExtensionsConfigurations, Action<SerializerRepository> customSerializerRegistrar = null)
        {
            redisExtensionsConfigurations.Serializer = new MsgPackObjectSerializer(customSerializerRegistrar);

            return redisExtensionsConfigurations;
        }
    }
}
