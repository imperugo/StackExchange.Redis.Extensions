// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database Factory useful in case of multiple instances of Redis.
/// </summary>
public interface IRedisClientFactory
{
    /// <summary>
    /// Return a list of all available Redis clients.
    /// </summary>
    /// <returns>A collection of all available <see cref="IRedisClient"/> instances.</returns>
    IEnumerable<IRedisClient> GetAllClients();

    /// <summary>
    /// Return the default instance of <see cref="IRedisClient"/>.
    /// </summary>
    /// <returns>The default <see cref="IRedisClient"/> instance.</returns>
    IRedisClient GetDefaultRedisClient();

    /// <summary>
    /// Return an instance of <see cref="IRedisClient"/>.
    /// </summary>
    /// <param name="name">If not specified returns the default instance</param>
    /// <returns>An instance of <see cref="IRedisClient"/>.</returns>
    IRedisClient GetRedisClient(string? name = null);

    /// <summary>
    /// Return the default instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    /// <returns>The default <see cref="IRedisDatabase"/> instance.</returns>
    IRedisDatabase GetDefaultRedisDatabase();

    /// <summary>
    /// Return an instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    /// <param name="name">If not specified returns the default instance</param>
    /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
    IRedisDatabase GetRedisDatabase(string? name = null);
}
