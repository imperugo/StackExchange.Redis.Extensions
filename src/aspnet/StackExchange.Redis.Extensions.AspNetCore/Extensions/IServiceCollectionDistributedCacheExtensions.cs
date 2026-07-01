// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis.Extensions.AspNetCore.Caching;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering <see cref="IDistributedCache"/> backed by StackExchange.Redis.Extensions.
/// </summary>
public static class IServiceCollectionDistributedCacheExtensions
{
    /// <summary>
    /// Registers <see cref="IDistributedCache"/> using the Redis connection managed by StackExchange.Redis.Extensions.
    /// Must be called after <see cref="IServiceCollectionExtensions.AddStackExchangeRedisExtensions{T}(IServiceCollection, StackExchange.Redis.Extensions.Core.Configuration.RedisConfiguration)"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedCache, RedisDistributedCache>();
        return services;
    }
}
