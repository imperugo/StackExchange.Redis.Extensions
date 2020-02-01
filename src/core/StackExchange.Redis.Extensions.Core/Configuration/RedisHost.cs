namespace StackExchange.Redis.Extensions.Core.Configuration
{
    /// <summary>
    /// This class represent the redis host configuration section
    /// </summary>
	public class RedisHost
    {
        /// <summary>
        /// The IP or host name of the redis server.
        /// </summary>
        /// <value>The IP or host name of the redis server.</value>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// The port of the redis server.
        /// </summary>
        /// <value>The port of the redis server.</value>
        public int Port { get; set; } = 6379;
    }
}
