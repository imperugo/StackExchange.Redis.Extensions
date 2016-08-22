using StackExchange.Redis.Extensions.Core.Interfaces;
using StackExchange.Redis.Extensions.Core.ServerIteration;

namespace StackExchange.Redis.Extensions.Core.Configurations
{
    public class RedisExtensionsConfigurations
    {
        public ISerializer Serializer { get; set; } = null;
        public ConfigurationOptions ConfigurationOptions { get; set; } = null;
        public ServerEnumerationStrategy ServerEnumerationStrategy { get; set; }

        public RedisExtensionsConfigurations()
        {

        }

        public RedisExtensionsConfigurations(ISerializer serializer, 
            ConfigurationOptions configurationOptions = null, 
            ServerEnumerationStrategy serverEnumerationStrategy = null)
        {
            Serializer = serializer;
            ConfigurationOptions = configurationOptions;
            ServerEnumerationStrategy = serverEnumerationStrategy;
        }
    }
}
