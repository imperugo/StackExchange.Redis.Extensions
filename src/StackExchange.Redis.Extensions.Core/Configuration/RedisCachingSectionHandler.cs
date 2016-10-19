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
			    var value = this["allowAdmin"]?.ToString();

                bool result;
                return !string.IsNullOrEmpty(value) && bool.TryParse(value, out result) && result;
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
			    var value = this["ssl"]?.ToString();

                bool result;
                return !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out result) && result;
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
			    var value = this["connectTimeout"]?.ToString();

                int result;
			    return !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out result) ? result : 5000;
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
			    var value = this["abortOnConnectFail"]?.ToString();

                bool result;
			    return !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out result) && result;
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
			    var value = this["database"]?.ToString();

			    int result;
			    return !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out result) ? result : 0;
			}
		}

		/// <summary>
		/// The password or access key
		/// </summary>
		[ConfigurationProperty("password", IsRequired = false)]
		public string Password => this["password"] as string;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static RedisCachingSectionHandler GetConfig() => ConfigurationManager.GetSection("redisCacheClient") as RedisCachingSectionHandler;
	}
}