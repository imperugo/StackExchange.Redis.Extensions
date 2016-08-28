using System;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core.Configurations;

namespace StackExchange.Redis.Extensions.Newtonsoft
{
    public static class ServiceCollectionExtensions
    {
        public static RedisExtensionsConfigurations WithNewtonsoft(this RedisExtensionsConfigurations redisExtensionsConfigurations, JsonSerializerSettings serializationSettings = null)
        {
            redisExtensionsConfigurations.Serializer = new NewtonsoftSerializer(serializationSettings);

            return redisExtensionsConfigurations;
        }

        public static RedisExtensionsConfigurations WithNewtonsoft(this RedisExtensionsConfigurations redisExtensionsConfigurations, Func<JsonSerializerSettings> serializationSettingsCreator)
        {
            WithNewtonsoft(redisExtensionsConfigurations, serializationSettingsCreator());

            return redisExtensionsConfigurations;
        }

        public static RedisExtensionsConfigurations WithNewtonsoft(this RedisExtensionsConfigurations redisExtensionsConfigurations, Action<JsonSerializerSettings> serializationSettingsCreator)
        {
            var jsonSerializerConfigurations = new JsonSerializerSettings();
            serializationSettingsCreator(jsonSerializerConfigurations);

            WithNewtonsoft(redisExtensionsConfigurations, jsonSerializerConfigurations);

            return redisExtensionsConfigurations;
        }
    }
}
