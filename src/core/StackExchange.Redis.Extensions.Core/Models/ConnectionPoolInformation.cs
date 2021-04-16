namespace StackExchange.Redis.Extensions.Core.Models
{
    /// <summary>
    /// A class that contains redis connection pool informations.
    /// </summary>
    public class ConnectionPoolInformation
    {
        /// <summary>
        /// Gets or sets the connection pool desiderated size.
        /// </summary>
        public int RequiredPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the number of active connections in the connection pool.
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Gets or sets the number of invalid connections in the connection pool.
        /// </summary>
        public int InvalidConnections { get; set; }

        /// <summary>
        /// Gets or sets the number ready connection, not used yet.
        /// </summary>
        public int ReadyNotUsedYet { get; set; }
    }
}
