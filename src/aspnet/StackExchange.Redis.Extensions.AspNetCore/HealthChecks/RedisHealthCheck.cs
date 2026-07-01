// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        cancellationToken.ThrowIfCancellationRequested();

        var data = new Dictionary<string, object>();
        var allHealthy = true;
        var anyHealthy = false;
        var clients = redisClientFactory.GetAllClients().ToList();

        if (clients.Count == 0)
            return HealthCheckResult.Unhealthy("No Redis clients are configured.", data: data);

        foreach (var client in clients)
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

            if (info.ActiveConnections > 0)
            {
                try
                {
                    var db = client.GetDefaultDatabase().Database;
                    var ping = await db.PingAsync().ConfigureAwait(false);
                    data[$"{name}:ping_ms"] = ping.TotalMilliseconds;
                }
                catch (RedisConnectionException ex)
                {
                    allHealthy = false;
                    data[$"{name}:ping"] = "connection_failed";
                    return HealthCheckResult.Unhealthy($"Redis PING failed for '{name}': connection error.", ex, data);
                }
                catch (RedisException ex)
                {
                    allHealthy = false;
                    data[$"{name}:ping"] = "failed";
                    return HealthCheckResult.Unhealthy($"Redis PING failed for '{name}'.", ex, data);
                }
                catch (RedisTimeoutException ex)
                {
                    allHealthy = false;
                    data[$"{name}:ping"] = "timed_out";
                    return HealthCheckResult.Unhealthy($"Redis PING timed out for '{name}'.", ex, data);
                }
            }
        }

        if (!anyHealthy)
            return HealthCheckResult.Unhealthy("All Redis connections are invalid.", data: data);

        if (!allHealthy)
            return HealthCheckResult.Degraded("Some Redis connections are invalid.", data: data);

        return HealthCheckResult.Healthy("All Redis connections are active.", data: data);
    }
}
