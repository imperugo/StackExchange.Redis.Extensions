using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
    public class RedisCachingSectionHandler : ConfigurationSection, IRedisCachingConfiguration
    {
        [ConfigurationProperty("hosts")]
        public RedisHostCollection RedisHosts
        {
            get
            {
                return this["hosts"] as RedisHostCollection;
            }
        }

        [ConfigurationProperty("allowAdmin")]
        public bool AllowAdmin
        {
            get
            {
                bool result = false;
                var o = this["allowAdmin"];

                if (o != null)
                {
                    var value = o.ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        if (bool.TryParse(value, out result))
                        {
                            return result;
                        }
                    }
                }

                return result;
            }
        }

        [ConfigurationProperty("ssl")]
        public bool Ssl
        {
            get
            {
                bool result = false;
                var o = this["ssl"];
                if (o != null)
                {
                    var value = o.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (bool.TryParse(value, out result))
                        {
                            return result;
                        }
                    }
                }

                return result;
            }
        }

        [ConfigurationProperty("connectTimeout")]
        public int ConnectTimeout
        {
            get
            {
                var o = this["connectTimeout"];
                if (o != null)
                {
                    var value = o.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int result;
                        if (int.TryParse(value, out result))
                        {
                            return result;
                        }
                    }
                }

                return 5000;
            }
        }

        public static RedisCachingSectionHandler GetConfig()
        {
            return ConfigurationManager.GetSection("redisCacheClient") as RedisCachingSectionHandler;
        }
    }
}