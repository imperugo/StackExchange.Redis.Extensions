using System;
using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
    public class RedisHost : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get
            {
                return this["host"] as string;
            }
        }

        [ConfigurationProperty("cachePort", IsRequired = true)]
        public int CachePort
        {
            get
            {
                var value = this["cachePort"].ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    int result;

                    if (int.TryParse(value, out result))
                    {
                        return result;
                    }
                }

                throw new Exception("Redis Cahe port must be number.");
            }
        }
    }
}