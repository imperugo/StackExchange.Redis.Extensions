using System;
using System.Collections.Concurrent;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    /// <inheritdoc/>
    public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private readonly ConcurrentBag<Lazy<StateAwareConnection>> connections;
        private readonly RedisConfiguration redisConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheConnectionPoolManager"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The redis configuration</param>
        public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration)
        {
            this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));

            this.connections = new ConcurrentBag<Lazy<StateAwareConnection>>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var activeConnections = this.connections.Where(lazy => lazy.IsValueCreated).ToList();

            for (var i = 0; i < activeConnections.Count; i++)
                activeConnections[i].Value.Invalidate();

            while (this.connections.IsEmpty == false)
                this.connections.TryTake(out var taken);
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            this.EmitConnections();

            var loadedLazies = this.connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazies.Count() == this.connections.Count)
                return (ConnectionMultiplexer)this.connections.OrderBy(x => x.Value.TotalOutstanding()).First().Value;

            return (ConnectionMultiplexer)this.connections.First(lazy => !lazy.IsValueCreated).Value;
        }

        private void EmitConnection()
        {
            this.connections.Add(new Lazy<StateAwareConnection>(() =>
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

                if (this.redisConfiguration.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

                return new StateAwareConnection(multiplexer, this.EmitConnection);
            }));
        }

        private void EmitConnections()
        {
            this.InvalidateDisconnectedConnections();

            var poolSize = this.redisConfiguration.PoolSize;

            var invalidOrDisconnectedConnections = this.connections.Count(lazy => lazy.IsValueCreated && (!lazy.Value.IsValid() || !lazy.Value.IsConnected()));

            var requiredNumOfConnections = poolSize - invalidOrDisconnectedConnections;

            if (invalidOrDisconnectedConnections <= 0 && this.connections.Count > 0)
                return;

            for (var i = 0; i < requiredNumOfConnections; i++)
                this.EmitConnection();
        }

        private void InvalidateDisconnectedConnections()
        {
            foreach (var lazy in connections)
            {
                if (lazy.IsValueCreated && !lazy.Value.IsConnected())
                    lazy.Value.Invalidate();
            }
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
            private bool invalidated;

            /// <summary>
            ///     Initializes a new instance of the <see cref="StateAwareConnection" /> class.
            /// </summary>
            /// <param name="multiplexer">The <see cref="ConnectionMultiplexer" /> connection object to observe.</param>
            /// <param name="connectionInvalidatedCallback">
            ///     A delegate representing a method that will be called when the give the connection became invalid.
            /// </param>
            public StateAwareConnection(ConnectionMultiplexer multiplexer, Action connectionInvalidatedCallback)
            {
                this.multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
                this.invalidateConnectionCallback = connectionInvalidatedCallback ?? throw new ArgumentNullException(nameof(connectionInvalidatedCallback));

                this.multiplexer.ConnectionFailed += this.ConnectionFailed;
            }

            public static implicit operator ConnectionMultiplexer(StateAwareConnection c) => c.multiplexer;

            public long TotalOutstanding() => this.multiplexer.GetCounters().TotalOutstanding;

            public void Invalidate()
            {
                if (this.invalidated)
                    return;

                this.invalidated = true;
                this.multiplexer.ConnectionFailed -= this.ConnectionFailed;
                this.multiplexer?.Dispose();
            }

            public bool IsConnected() => this.multiplexer.IsConnecting == false;

            public bool IsValid() => this.invalidated == false;

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
                            this.Invalidate();
                            this.invalidateConnectionCallback();
                            break;
                        }
                }
            }
        }
    }
}
