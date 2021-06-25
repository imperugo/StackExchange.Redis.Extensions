using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;

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
        private readonly IStateAwareConnection[] connections;
        private readonly RedisConfiguration redisConfiguration;
        private readonly ILogger<RedisCacheConnectionPoolManager> logger;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);
        private static readonly object @lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheConnectionPoolManager"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <param name="logger">The logger.</param>
        public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisCacheConnectionPoolManager> logger = null)
        {
            this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
            this.logger = logger ?? NullLogger<RedisCacheConnectionPoolManager>.Instance;

            lock (@lock)
            {
                this.connections = new IStateAwareConnection[redisConfiguration.PoolSize];
                this.EmitConnections();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                // free managed resources
                foreach (var connection in this.connections)
                    connection.Dispose();
            }

            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            var connection = this.connections.OrderBy(x => x.TotalOutstanding()).First();

            logger.LogDebug("Using connection {0} with {1} outstanding!", connection.Connection.GetHashCode(), connection.TotalOutstanding());

            return connection.Connection;
        }

        /// <inheritdoc/>
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;

            foreach (var connection in connections)
            {
                if (!connection.IsConnected())
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
                InvalidConnections = invalidConnections
            };
        }

        private void EmitConnections()
        {
            for (var i = 0; i < this.redisConfiguration.PoolSize; i++)
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

                if (this.redisConfiguration.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

                this.connections[i] = this.redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
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
                this.Connection.InternalError += this.InternalError;
                this.Connection.ErrorMessage += this.ErrorMessage;

                this.logger = logger;
            }

            public IConnectionMultiplexer Connection { get; }

            public long TotalOutstanding() => this.Connection.GetCounters().TotalOutstanding;

            public bool IsConnected() => !this.Connection.IsConnecting;

            public void Dispose()
            {
                this.Connection.ConnectionFailed -= ConnectionFailed;
                this.Connection.ConnectionRestored -= ConnectionRestored;
                this.Connection.InternalError -= this.InternalError;
                this.Connection.ErrorMessage -= this.ErrorMessage;

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

            private void InternalError(object sender, InternalErrorEventArgs e)
            {
                logger.LogError(e.Exception, "Redis internal error {0}.", e.Origin);
            }

            private void ErrorMessage(object sender, RedisErrorEventArgs e)
            {
                logger.LogError("Redis error: " + e.Message);
            }
        }
    }
}
