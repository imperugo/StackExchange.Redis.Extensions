// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.AspNetCore.Middlewares;

internal sealed class RedisInformationMiddleware(
    RequestDelegate next,
    RedisMiddlewareAccessOptions options,
    ILogger<RedisInformationMiddleware> logger,
    IRedisClientFactory redisClientFactory,
    IRedisDatabase redisDatabase)
{
#pragma warning disable RCS1046
    public async Task Invoke(HttpContext context)
#pragma warning restore RCS1046
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("{MiddlewareName} --> Handling request: {Path}", nameof(RedisInformationMiddleware), context.Request.Path);

        if (context.Request.Method == "GET" && context.Request.Path == "/redis/connectionInfo")
        {
            if (!IsClientAllowed(context))
            {
                await next.Invoke(context).ConfigureAwait(false);
                return;
            }

            var clients = redisClientFactory.GetAllClients();

            var list = new List<ConnectionPoolInformation>();

            foreach (var client in clients)
                list.Add(client.ConnectionPoolManager.GetConnectionInformation());

            await JsonSerializer.SerializeAsync(context.Response.Body, list).ConfigureAwait(false);

            return;
        }

        if (context.Request.Method == "GET" && context.Request.Path == "/redis/info")
        {
            if (!IsClientAllowed(context))
            {
                await next.Invoke(context).ConfigureAwait(false);
                return;
            }

            var data = await redisDatabase.GetInfoAsync().ConfigureAwait(false);

            await JsonSerializer.SerializeAsync(context.Response.Body, data).ConfigureAwait(false);

            return;
        }

        await next.Invoke(context).ConfigureAwait(false);
    }

    private bool IsClientAllowed(HttpContext context)
    {
        // Make `AllowFunction` higher priority so that users can use `AllowedIPs` in their own logic.
        var allowFunc = options.AllowFunction;
        if (allowFunc != null)
            return allowFunc.Invoke(context);

        var allowIPs = options.AllowedIPs;

        if (allowIPs is { Length: > 0 })
            return Array.Exists(allowIPs, p => p.Equals(context.Connection.RemoteIpAddress));

        return false;
    }
}
