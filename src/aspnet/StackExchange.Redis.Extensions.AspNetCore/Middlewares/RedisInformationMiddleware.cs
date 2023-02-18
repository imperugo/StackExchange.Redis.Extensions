// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.AspNetCore.Middlewares;

internal class RedisInformationMiddleware
{
    private readonly ILogger<RedisInformationMiddleware> logger;
    private readonly RequestDelegate next;
    private readonly IRedisClientFactory redisClientFactory;
    private readonly IRedisDatabase redisDatabase;
    private readonly RedisMiddlewareAccessOptions options;

    public RedisInformationMiddleware(
        RequestDelegate next,
        RedisMiddlewareAccessOptions options,
        ILogger<RedisInformationMiddleware> logger,
        IRedisClientFactory redisClientFactory,
        IRedisDatabase redisDatabase)
    {
        this.next = next;
        this.options = options;
        this.logger = logger;
        this.redisClientFactory = redisClientFactory;
        this.redisDatabase = redisDatabase;
    }

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
        if (options.AllowedIPs == null)
            return true;

        if (options.AllowedIPs.Any(x => x.Equals(context.Connection.RemoteIpAddress)))
            return true;

        if (options.AllowFunction != null)
            return options.AllowFunction(context);

        return false;
    }
}
