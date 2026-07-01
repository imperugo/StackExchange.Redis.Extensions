// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.AspNetCore.HealthChecks;

internal sealed class RedisHealthCheck(IRedisClientFactory redisClientFactory) : IHealthCheck
{
    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var allHealthy = true;
        var anyHealthy = false;

        foreach (var client in redisClientFactory.GetAllClients())
        {
            var name = client.Name;
            var info = client.ConnectionPoolManager.GetConnectionInformation();

            data[$"{name}:active_connections"] = info.ActiveConnections;
            data[$"{name}:invalid_connections"] = info.InvalidConnections;
            data[$"{name}:required_pool_size"] = info.RequiredPoolSize;

            if (info.InvalidConnections > 0)
                allHealthy = false;

            if (info.ActiveConnections > 0)
                anyHealthy = true;
        }

        if (!anyHealthy)
            return HealthCheckResult.Unhealthy("All Redis connections are invalid.", data: data);

        try
        {
            var db = redisClientFactory.GetDefaultRedisDatabase().Database;
            var ping = await db.PingAsync().ConfigureAwait(false);
            data["ping_ms"] = ping.TotalMilliseconds;
        }
        catch (RedisException ex)
        {
            return HealthCheckResult.Unhealthy("Redis PING failed.", ex, data);
        }
        catch (RedisTimeoutException ex)
        {
            return HealthCheckResult.Unhealthy("Redis PING timed out.", ex, data);
        }

        if (!allHealthy)
            return HealthCheckResult.Degraded("Some Redis connections are invalid.", data: data);

        return HealthCheckResult.Healthy("All Redis connections are active.", data: data);
    }
}
