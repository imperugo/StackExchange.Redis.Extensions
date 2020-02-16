namespace StackExchange.Redis.Extensions.Core.Implementations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Configuration;

    public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private readonly ConcurrentBag<Lazy<StateAwareConnection>> connections;
        private readonly RedisConfiguration redisConfiguration;

        public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration)
        {
            this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));

            this.connections = new ConcurrentBag<Lazy<StateAwareConnection>>();
        }

        public void Dispose()
        {
            List<Lazy<StateAwareConnection>> activeConnections = this.connections.Where(lazy => lazy.IsValueCreated).ToList();

            foreach (Lazy<StateAwareConnection> connection in activeConnections) connection.Value.Invalidate();

            while (this.connections.IsEmpty == false) this.connections.TryTake(out Lazy<StateAwareConnection> taken);
        }

        public IConnectionMultiplexer GetConnection()
        {
            Lazy<StateAwareConnection> response;
            IEnumerable<Lazy<StateAwareConnection>> loadedLazies = this.connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazies.Count() == this.connections.Count)
                response = this.connections.OrderBy(x => x.Value.TotalOutstanding()).First();
            else
                response = this.connections.First(lazy => !lazy.IsValueCreated);

            ConnectionMultiplexer connectionMultiplexer = response.Value;
            return connectionMultiplexer;
        }

        private void Initialize()
        {
            void AddConnection()
            {
                ConfigurationOptions configurationOptions = this.redisConfiguration.ConfigurationOptions;
                ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                if (this.redisConfiguration.ProfilingSessionProvider != null) multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

                StateAwareConnection InitializeConnection() => new StateAwareConnection(multiplexer, AddConnection);
                this.connections.Add(new Lazy<StateAwareConnection>(InitializeConnection));
            }

            for (var i = 0; i < this.redisConfiguration.PoolSize; i++) AddConnection();
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

            public long TotalOutstanding() => this.multiplexer.GetCounters().TotalOutstanding;

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

            public static implicit operator ConnectionMultiplexer(StateAwareConnection c) => c.multiplexer;

            public void Invalidate()
            {
                if (this.invalidated)
                    return;

                this.invalidated = true;
                this.multiplexer.ConnectionFailed -= this.ConnectionFailed;
                this.multiplexer?.Dispose();
            }
        }
    }
}