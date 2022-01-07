// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The service who handles the Redis connection pool.
/// </summary>
public interface IRedisConnectionPoolManager : IDisposable
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
