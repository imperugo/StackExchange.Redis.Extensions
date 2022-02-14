// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public sealed partial class RedisConnectionPoolManager
{
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
            Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
            Connection.ConnectionFailed += ConnectionFailed;
            Connection.ConnectionRestored += ConnectionRestored;
            Connection.InternalError += InternalError;
            Connection.ErrorMessage += ErrorMessage;

            this.logger = logger;
        }

        public IConnectionMultiplexer Connection { get; }

        public long TotalOutstanding() => Connection.GetCounters().TotalOutstanding;

        public bool IsConnected() => !Connection.IsConnecting;

        public void Dispose()
        {
            Connection.ConnectionFailed -= ConnectionFailed;
            Connection.ConnectionRestored -= ConnectionRestored;
            Connection.InternalError -= InternalError;
            Connection.ErrorMessage -= ErrorMessage;

            Connection.Dispose();
        }

        private void ConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            logger.LogError(e.Exception, "Redis connection error {FailureType}", e.FailureType.ToString());
        }

        private void ConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        {
            logger.LogInformation("Redis connection error restored");
        }

        private void InternalError(object? sender, InternalErrorEventArgs e)
        {
            logger.LogError(e.Exception, "Redis internal error {Origin}", e.Origin);
        }

        private void ErrorMessage(object? sender, RedisErrorEventArgs e)
        {
            logger.LogError("Redis error: {Message}", e.Message);
        }
    }
}
