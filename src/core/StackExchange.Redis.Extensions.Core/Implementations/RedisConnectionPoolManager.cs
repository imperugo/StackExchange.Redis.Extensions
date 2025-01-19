// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
/* Unmerged change from project 'StackExchange.Redis.Extensions.Core(net6.0)'
Before:
using System.Linq;
using System.Threading.Tasks;
After:
using System.Threading.Tasks;
*/

/* Unmerged change from project 'StackExchange.Redis.Extensions.Core(net7.0)'
Before:
using System.Linq;
using System.Threading.Tasks;
After:
using System.Threading.Tasks;
*/

/* Unmerged change from project 'StackExchange.Redis.Extensions.Core(net8.0)'
Before:
using System.Linq;
using System.Threading.Tasks;
After:
using System.Threading.Tasks;
*/
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public sealed partial class RedisConnectionPoolManager : IRedisConnectionPoolManager
{
    private static readonly object @lock = new();
    private readonly IStateAwareConnection[] connections;
    private readonly RedisConfiguration redisConfiguration;
    private readonly ILogger<RedisConnectionPoolManager> logger;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConnectionPoolManager"/> class.
    /// </summary>
    /// <param name="redisConfiguration">The redis configuration.</param>
    /// <param name="logger">The logger. If null will create one from redisConfiguration.LoggerFactory if factory provided</param>
    public RedisConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisConnectionPoolManager>? logger = null)
    {
        this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        logger ??= redisConfiguration.LoggerFactory?.CreateLogger<RedisConnectionPoolManager>();
        this.logger = logger ?? NullLogger<RedisConnectionPoolManager>.Instance;

        lock (@lock)
        {
            connections = new IStateAwareConnection[redisConfiguration.PoolSize];
            EmitConnections();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            // free managed resources
            foreach (var connection in connections)
                connection.Dispose();
        }

        isDisposed = true;
    }

    /// <inheritdoc/>
    public IConnectionMultiplexer GetConnection()
    {
        IStateAwareConnection connection;

        switch (redisConfiguration.ConnectionSelectionStrategy)
        {
            case ConnectionSelectionStrategy.RoundRobin:
                var nextIdx
#if NET6_0_OR_GREATER
                = RandomNumberGenerator.GetInt32(0, redisConfiguration.PoolSize);
#else
                = new Random().Next(0, redisConfiguration.PoolSize);
#endif
                connection = connections[nextIdx];
                break;

            case ConnectionSelectionStrategy.LeastLoaded:
                connection = ValueLengthExtensions.MinBy(connections, x => x.TotalOutstanding());
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(redisConfiguration.ConnectionSelectionStrategy), (int)redisConfiguration.ConnectionSelectionStrategy, typeof(ConnectionSelectionStrategy));
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Using connection {HashCode} with {OutStanding} outstanding!", connection.Connection.GetHashCode().ToString(CultureInfo.InvariantCulture), connection.TotalOutstanding().ToString(CultureInfo.InvariantCulture));

        return connection.Connection;
    }

    /// <inheritdoc/>
    public IEnumerable<IConnectionMultiplexer> GetConnections()
    {
        foreach (var connection in connections)
            yield return connection.Connection;
    }

    /// <inheritdoc/>
    public ConnectionPoolInformation GetConnectionInformation()
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

        return new()
        {
            RequiredPoolSize = redisConfiguration.PoolSize,
            ActiveConnections = activeConnections,
            InvalidConnections = invalidConnections
        };
    }

    private void EmitConnections()
    {
        Parallel.For(0, redisConfiguration.PoolSize, index =>
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

            if (redisConfiguration.ProfilingSessionProvider != null)
                multiplexer.RegisterProfiler(redisConfiguration.ProfilingSessionProvider);

            connections[index] = redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
        });
    }
}
