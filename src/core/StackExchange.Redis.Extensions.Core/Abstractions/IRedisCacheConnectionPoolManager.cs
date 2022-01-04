using System;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The service who handles the Redis connection pool.
/// </summary>
public interface IRedisCacheConnectionPoolManager : IDisposable
{
    /// <summary>
    /// Get the Redis connection
    /// </summary>
    /// <returns>Returns an instance of<see cref="IConnectionMultiplexer"/>.</returns>
    IConnectionMultiplexer GetConnection();

    /// <summary>
    ///     Gets the information about the connection pool
    /// </summary>
    ConnectionPoolInformation GetConnectionInformations();
}
