using System;
using System.Collections.Concurrent;
using System.Linq;

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
        private readonly ConcurrentBag<Lazy<IStateAwareConnection>> connections;
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
            this.connections = new ConcurrentBag<Lazy<IStateAwareConnection>>();
            this.logger = logger ?? NullLogger<RedisCacheConnectionPoolManager>.Instance;
            this.EmitConnections();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var activeConnections = this.connections.Where(lazy => lazy.IsValueCreated).ToList();

            foreach (var connection in activeConnections)
                connection.Value.Dispose();

            while (this.connections.IsEmpty == false)
                this.connections.TryTake(out var taken);
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            var loadedLazies = this.connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazies.Count() == this.connections.Count)
                return this.connections.OrderBy(x => x.Value.TotalOutstanding()).First().Value.Connection;

            return this.connections.First(lazy => !lazy.IsValueCreated).Value.Connection;
        }

        /// <inheritdoc/>
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;
            var readyNotUsedYet = 0;

            foreach (var lazy in connections)
            {
                if (!lazy.IsValueCreated)
                {
                    readyNotUsedYet++;
                    continue;
                }

                if (!lazy.Value.IsConnected())
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
                ReadyNotUsedYet = readyNotUsedYet
            };
        }

        private void EmitConnection()
        {
            this.connections.Add(new Lazy<IStateAwareConnection>(() =>
            {
                this.logger.LogDebug("Creating new Redis connection.");

                var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

                if (this.redisConfiguration.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

                return this.redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
            }));
        }

        private void EmitConnections()
        {
            if (connections.Count >= this.redisConfiguration.PoolSize)
                return;

            for (var i = 0; i < this.redisConfiguration.PoolSize; i++)
            {
                logger.LogDebug("Creating the redis connection pool with {0} connections.", this.redisConfiguration.PoolSize);
                this.EmitConnection();
            }
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
