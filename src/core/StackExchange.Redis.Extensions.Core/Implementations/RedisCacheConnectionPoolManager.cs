using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    /// <inheritdoc/>
    public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private readonly ConcurrentBag<IStateAwareConnection> connections;
        private readonly RedisConfiguration redisConfiguration;
        private readonly ILogger<RedisCacheConnectionPoolManager> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheConnectionPoolManager"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <param name="logger">The logger.</param>
        public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisCacheConnectionPoolManager> logger = null)
        {
            this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
            this.connections = new ConcurrentBag<IStateAwareConnection>();
            this.logger = logger ?? NullLogger<RedisCacheConnectionPoolManager>.Instance;
            this.EmitConnections();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var connection in connections)
                connection.Dispose();

            while (this.connections.IsEmpty == false)
                this.connections.TryTake(out var taken);
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            if (this.connections.IsEmpty == false)
            {
                return this.connections.OrderBy(c => c.TotalOutstanding()).First().Connection;
            }

            throw new Exception("no connection available");
        }

        /// <inheritdoc/>
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;

            foreach (var lazy in connections)
            {
                if (!lazy.IsConnected())
                {
                    invalidConnections++;
                    continue;
                }

                activeConnections++;
            }

            return new ConnectionPoolInformation()
            {
                RequiredPoolSize = redisConfiguration.PoolSize,
                ActiveConnections = activeConnections,
                InvalidConnections = invalidConnections,
                ReadyNotUsedYet = 0
            };
        }

        private Task EmitConnection()
        {
            return Task.Run(
                async () =>
                {
                    this.logger.LogDebug("Creating new Redis connection.");
                    var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConfiguration.ConfigurationOptions);
                    if (this.redisConfiguration.ProfilingSessionProvider != null)
                        multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);
                    this.connections.Add(this.redisConfiguration.StateAwareConnectionFactory(multiplexer, logger));
                });
        }

        private void EmitConnections()
        {
            logger.LogDebug("Creating the redis connection pool with {0} connections.", this.redisConfiguration.PoolSize);
            var tasks = Enumerable.Range(0, this.redisConfiguration.PoolSize)
                .Select(_ => this.EmitConnection())
                .ToArray();
            Task.WaitAny(tasks);    // wait for at least 1 connection to be available
        }

        /// <summary>
        ///     Wraps a <see cref="ConnectionMultiplexer" /> instance. Subscribes to certain events of the
        ///     <see cref="ConnectionMultiplexer" /> object and invalidates it in case the connection transients into a state to be
        ///     considered as permanently disconnected.
        /// </summary>
        internal sealed class StateAwareConnection : IStateAwareConnection
        {
            private readonly ILogger logger;

            /// <summary>
            ///     Initializes a new instance of the <see cref="StateAwareConnection" /> class.
            /// </summary>
            /// <param name="multiplexer">The <see cref="ConnectionMultiplexer" /> connection object to observe.</param>
            /// <param name="logger">The logger.</param>
            public StateAwareConnection(IConnectionMultiplexer multiplexer, ILogger logger)
            {
                this.Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
                this.Connection.ConnectionFailed += this.ConnectionFailed;
                this.Connection.ConnectionRestored += this.ConnectionRestored;

                this.logger = logger;
            }

            public IConnectionMultiplexer Connection { get; private set; }

            public long TotalOutstanding() => this.Connection.GetCounters().TotalOutstanding;

            public bool IsConnected() => this.Connection.IsConnecting == false;

            public void Dispose()
            {
                this.Connection.ConnectionFailed -= ConnectionFailed;
                this.Connection.ConnectionRestored -= ConnectionRestored;

                Connection.Dispose();
            }

            private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
            {
                logger.LogError(e.Exception, "Redis connection error {0}.", e.FailureType);
            }

            private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
            {
                logger.LogError("Redis connection error restored.");
            }
        }
    }
}
