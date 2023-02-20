// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Samples.Web.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly IRedisDatabase redisDatabase;
    private readonly IRedisConnectionPoolManager pool;

    public HomeController(IRedisDatabase redisDatabase, IRedisConnectionPoolManager pool)
    {
        this.redisDatabase = redisDatabase;
        this.pool = pool;
    }

    public async Task<string> IndexAsync()
    {
        var before = pool.GetConnectionInformation();
        var rng = new Random();
        await redisDatabase.AddAsync($"key-{rng}", new { a = rng.Next() }).ConfigureAwait(false);

        var after = pool.GetConnectionInformation();

        return BuildInfo(before) + "\t" + BuildInfo(after);

        static string BuildInfo(ConnectionPoolInformation info)
        {
            return $"\talive: {info.ActiveConnections.ToString()}, required: {info.RequiredPoolSize.ToString()}";
        }
    }
}
