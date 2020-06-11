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
        private readonly ConcurrentBag<Lazy<StateAwareConnection>> connections;
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

            this.connections = new ConcurrentBag<Lazy<StateAwareConnection>>();
            this.logger = logger ?? NullLogger<RedisCacheConnectionPoolManager>.Instance;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var activeConnections = this.connections.Where(lazy => lazy.IsValueCreated).ToList();

            logger.LogDebug("Disposing {0} active connections.", activeConnections.Count);

            for (var i = 0; i < activeConnections.Count; i++)
                activeConnections[i].Value.Invalidate();

            while (this.connections.IsEmpty == false)
            {
                logger.LogDebug("Removing invalid connections from pool.");
                this.connections.TryTake(out var taken);
            }
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            this.EmitConnections();

            var loadedLazies = this.connections.Where(lazy => lazy.IsValueCreated && lazy.Value.IsValid() && lazy.Value.IsConnected());

            if (loadedLazies.Count() == this.connections.Count)
                return (ConnectionMultiplexer)this.connections.OrderBy(x => x.Value.TotalOutstanding()).First().Value;

            var connection = this.connections.FirstOrDefault(lazy => !lazy.IsValueCreated);

            if (connection != null)
                return (ConnectionMultiplexer)connection.Value;

            return (ConnectionMultiplexer)loadedLazies.First().Value;
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

                if (!lazy.Value.IsValid())
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
            this.connections.Add(new Lazy<StateAwareConnection>(() =>
            {
                this.logger.LogDebug("Creating new Redis connection.");

                var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

                if (this.redisConfiguration.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

                if (redisConfiguration.IsSentinelCluster)
                    multiplexer = multiplexer.GetSentinelMasterConnection(redisConfiguration.ConfigurationOptions);

                return new StateAwareConnection(multiplexer, this.EmitConnection, logger);
            }));
        }

        private void EmitConnections()
        {
            if (connections.Count > 0)
                return;

            logger.LogDebug("Inizializing connection pool.");

            for (var i = 0; i < this.redisConfiguration.PoolSize; i++)
                this.EmitConnection();
        }

        /// <summary>
        ///     Wraps a <see cref="ConnectionMultiplexer" /> instance. Subscribes to certain events of the
        ///     <see cref="ConnectionMultiplexer" /> object and invalidates it in case the connection transients into a state to be
        ///     considered as permanently disconnected.
        /// </summary>
        internal sealed class StateAwareConnection
        {
            private readonly Action invalidateConnectionCallback;
            private readonly ConnectionMultiplexer multiplexer;
            private readonly ILogger logger;
            private bool isDisposed = false;
            private bool isAvailable = true;

            /// <summary>
            ///     Initializes a new instance of the <see cref="StateAwareConnection" /> class.
            /// </summary>
            /// <param name="multiplexer">The <see cref="ConnectionMultiplexer" /> connection object to observe.</param>
            /// <param name="connectionInvalidatedCallback">
            ///     A delegate representing a method that will be called when the give the connection became invalid.
            /// </param>
            /// <param name="logger">The logger.</param>
            public StateAwareConnection(ConnectionMultiplexer multiplexer, Action connectionInvalidatedCallback, ILogger logger)
            {
                this.multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
                this.invalidateConnectionCallback = connectionInvalidatedCallback ?? throw new ArgumentNullException(nameof(connectionInvalidatedCallback));

                this.multiplexer.ConnectionFailed += this.ConnectionFailed;
                this.multiplexer.ConnectionRestored += this.ConnectionRestored;
                this.logger = logger;
            }

            public static implicit operator ConnectionMultiplexer(StateAwareConnection c) => c.multiplexer;

            public long TotalOutstanding() => this.multiplexer.GetCounters().TotalOutstanding;

            public void Invalidate()
            {
                logger.LogWarning("Invalidating redis connection...");

                if (!isAvailable || isDisposed)
                    return;

                this.multiplexer.ConnectionFailed -= this.ConnectionFailed;
                this.multiplexer.ConnectionRestored -= this.ConnectionRestored;
                this.multiplexer?.Dispose();
                this.isDisposed = true;
            }

            public bool IsValid()
            {
                if (isAvailable && !isDisposed)
                    return true;

                return false;
            }

            public bool IsConnected() => this.multiplexer.IsConnected;

            private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
            {
                switch (e.FailureType)
                {
                    case ConnectionFailureType.ConnectionDisposed:
                    case ConnectionFailureType.InternalFailure:
                    case ConnectionFailureType.SocketClosed:
                    case ConnectionFailureType.SocketFailure:
                    case ConnectionFailureType.UnableToConnect:
                        {
                            logger.LogError(e.Exception, "Redis connection error {0}.", e.FailureType);

                            this.isAvailable = false;

                            this.invalidateConnectionCallback();
                            break;
                        }
                }
            }

            private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
            {
                logger.LogError(e.Exception, "Redis automatically reconnect on error {0}.", e.FailureType);

                switch (e.FailureType)
                {
                    case ConnectionFailureType.None:
                        {
                            logger.LogDebug("Redis connection restored successfully.");

                            this.isAvailable = true;
                            break;
                        }

                    default:
                        {
                            logger.LogError(e.Exception, "An error connection during reconnecting `{0}`. Disposing the connection.", e.FailureType);

                            this.Invalidate();
                            this.invalidateConnectionCallback();
                            break;
                        }
                }
            }
        }
    }
}
