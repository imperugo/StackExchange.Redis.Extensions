using StackExchange.Redis.Extensions.Core.Interfaces;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Configurations
{
    public class RedisExtensionsConfigurations
    {
        public ISerializer Serializer { get; set; } = null;
    }
}
