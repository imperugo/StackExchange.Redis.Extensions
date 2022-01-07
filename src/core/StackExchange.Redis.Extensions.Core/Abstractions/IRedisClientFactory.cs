// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database Factory usefull in case of multplie instances of Redis.
/// </summary>
public interface IRedisClientFactory
{
    /// <summary>
    /// Return a list of all available Redis clients.
    /// </summary>
    IEnumerable<IRedisClient> GetAllClients();

    /// <summary>
    /// Return the default instance of <see cref="IRedisClient"/>.
    /// </summary>
    IRedisClient GetDefaultRedisClient();

    /// <summary>
    /// Return an instance of <see cref="IRedisClient"/>.
    /// </summary>
    /// <param name="name">If not specified returns the default instance</param>
    IRedisClient GetRedisClient(string? name = null);

    /// <summary>
    /// Return the default instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    IRedisDatabase GetDefaultRedisDatabase();

    /// <summary>
    /// Return an instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    /// <param name="name">If not specified returns the default instance</param>
    IRedisDatabase GetRedisDatabase(string? name = null);
}

