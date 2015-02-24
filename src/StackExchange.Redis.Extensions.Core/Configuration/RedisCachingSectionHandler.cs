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
				var config = this["allowAdmin"];

				if (config != null)
				{
					var value = config.ToString();

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
				var config = this["ssl"];
				if (config != null)
				{
					var value = config.ToString();
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
				var config = this["connectTimeout"];
				if (config != null)
				{
					var value = config.ToString();
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

		[ConfigurationProperty("db")]
		public int Db
		{
			get
			{
				var config = this["db"];
				if (config != null)
				{
					var value = config.ToString();
					if (!string.IsNullOrWhiteSpace(value))
					{
						int result;
						if (int.TryParse(value, out result))
						{
							return result;
						}
					}
				}

				return 0;
			}
		}

		public static RedisCachingSectionHandler GetConfig()
		{
			return ConfigurationManager.GetSection("redisCacheClient") as RedisCachingSectionHandler;
		}
	}
}