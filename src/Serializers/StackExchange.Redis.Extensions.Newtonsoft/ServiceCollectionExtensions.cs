using System;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core.Configurations;

namespace StackExchange.Redis.Extensions.Newtonsoft
{
    public static class ServiceCollectionExtensions
    {
        public static RedisExtensionsConfigurations WithNewtonsoft(this RedisExtensionsConfigurations configurator, JsonSerializerSettings serializationSettings = null)
        {
            configurator.Serializer = new NewtonsoftSerializer(serializationSettings);

            return configurator;
        }

        public static RedisExtensionsConfigurations WithNewtonsoft(this RedisExtensionsConfigurations configurator, Func<JsonSerializerSettings> serializationSettingsCreator)
        {
            WithNewtonsoft(configurator, serializationSettingsCreator());

            return configurator;
        }
    }
}
