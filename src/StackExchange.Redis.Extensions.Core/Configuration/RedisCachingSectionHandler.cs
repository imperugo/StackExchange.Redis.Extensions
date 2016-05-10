using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
	/// <summary>
	/// The implementation of <see cref="IRedisCachingConfiguration"/>
	/// </summary>
	public class RedisCachingSectionHandler : ConfigurationSection, IRedisCachingConfiguration
	{
		/// <summary>
		/// The host of Redis Server
		/// </summary>
		/// <value>
		/// The ip or name
		/// </value>
		[ConfigurationProperty("hosts")]
		public RedisHostCollection RedisHosts
			=> this["hosts"] as RedisHostCollection;

		/// <summary>
		/// The strategy to use when executing server wide commands
		/// </summary>
		[ConfigurationProperty("serverEnumerationStrategy")]
		public ServerEnumerationStrategy ServerEnumerationStrategy
			=> this["serverEnumerationStrategy"] as ServerEnumerationStrategy;
		

		/// <summary>
		/// Specify if the connection can use Admin commands like flush database
		/// </summary>
		/// <value>
		///   <c>true</c> if can use admin commands; otherwise, <c>false</c>.
		/// </value>
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

		/// <summary>
		/// Specify if the connection is a secure connection or not.
		/// </summary>
		/// <value>
		///   <c>true</c> if is secure; otherwise, <c>false</c>.
		/// </value>
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

		/// <summary>
		/// The connection timeout
		/// </summary>
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

		/// <summary>
		/// If true, Connect will not create a connection while no servers are available
		/// </summary>
		[ConfigurationProperty("abortOnConnectFail")]
		public bool AbortOnConnectFail
		{
			get
			{
				var config = this["abortOnConnectFail"];
				if (config != null)
				{
					var value = config.ToString();
					if (!string.IsNullOrWhiteSpace(value))
					{
						bool result;
						if (bool.TryParse(value, out result))
						{
							return result;
						}
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Database Id
		/// </summary>
		/// <value>
		/// The database id, the default value is 0
		/// </value>
		[ConfigurationProperty("database")]
		public int Database
		{
			get
			{
				var config = this["database"];
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

		/// <summary>
		/// The password or access key
		/// </summary>
		[ConfigurationProperty("password", IsRequired = false)]
		public string Password
		{
			get
			{
				return this["password"] as string;
			}
		}

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <returns></returns>
		public static RedisCachingSectionHandler GetConfig()
		{
			return ConfigurationManager.GetSection("redisCacheClient") as RedisCachingSectionHandler;
		}
	}
}