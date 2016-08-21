namespace StackExchange.Redis.Extensions.Core.Configuration
{
	/// <summary>
	/// Handle Redis Configuration
	/// </summary>
	public interface IRedisCachingConfiguration
	{
		/// <summary>
		/// The host of Redis Server
		/// </summary>
		/// <value>
		/// The ip or name
		/// </value>
		RedisHostCollection RedisHosts { get; }

		/// <summary>
		/// The strategy to use when executing server wide commands
		/// </summary>
		ServerEnumerationStrategy ServerEnumerationStrategy { get; }

		/// <summary>
		/// Specify if the connection can use Admin commands like flush database
		/// </summary>
		/// <value>
		///   <c>true</c> if can use admin commands; otherwise, <c>false</c>.
		/// </value>
		bool AllowAdmin { get; }

		/// <summary>
		/// Specify if the connection is a secure connection or not.
		/// </summary>
		/// <value>
		///   <c>true</c> if is secure; otherwise, <c>false</c>.
		/// </value>
		bool Ssl { get; }

		/// <summary>
		/// The connection timeout
		/// </summary>
		int ConnectTimeout { get; }

		/// <summary>
		/// If true, Connect will not create a connection while no servers are available
		/// </summary>
		bool AbortOnConnectFail { get; }

		/// <summary>
		/// Database Id
		/// </summary>
		/// <value>
		/// The database id, the default value is 0
		/// </value>
		int Database { get; }


        /// <summary>
        /// The password or access key
        /// </summary>
        string Password { get; }
    }
}