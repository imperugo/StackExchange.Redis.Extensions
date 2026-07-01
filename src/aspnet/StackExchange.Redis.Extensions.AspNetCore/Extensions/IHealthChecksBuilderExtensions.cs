// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using StackExchange.Redis.Extensions.AspNetCore.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Redis health checks.
/// </summary>
public static class IHealthChecksBuilderExtensions
{
    /// <summary>
    /// Adds a health check for the Redis connection pool managed by StackExchange.Redis.Extensions.
    /// The check verifies connection pool status across all registered Redis instances and performs a PING.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check (default: "redis").</param>
    /// <param name="failureStatus">The <see cref="HealthStatus"/> to report when the health check fails (default: <see cref="HealthStatus.Unhealthy"/>).</param>
    /// <param name="tags">Optional tags for filtering health checks.</param>
    /// <param name="timeout">Optional timeout for the health check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddRedisExtensionsHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "redis",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        System.TimeSpan? timeout = null)
    {
        return builder.AddCheck<RedisHealthCheck>(name, failureStatus, tags, timeout);
    }
}
