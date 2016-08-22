using StackExchange.Redis.Extensions.Core.Interfaces;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Configurations
{
    public class RedisExtensionsConfigurations
    {
        public ISerializer Serializer { get; set; } = null;
        public ConfigurationOptions ConfigurationOptions { get; set; } = null;

        public RedisExtensionsConfigurations()
        {

        }

        public RedisExtensionsConfigurations(ISerializer serializer, ConfigurationOptions configurationOptions)
        {
            Serializer = serializer;
            ConfigurationOptions = configurationOptions;
        }
    }
}
