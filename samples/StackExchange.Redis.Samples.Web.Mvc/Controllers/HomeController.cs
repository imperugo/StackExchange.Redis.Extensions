using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Samples.Web.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly IRedisDatabase redisDatabase;
    private readonly IRedisCacheConnectionPoolManager pool;

    public HomeController(IRedisDatabase redisDatabase, IRedisCacheConnectionPoolManager pool)
    {
        this.redisDatabase = redisDatabase;
        this.pool = pool;
    }

    public async Task<string> Index()
    {
        var before = pool.GetConnectionInformations();
        var rng = new Random();
        await redisDatabase.AddAsync($"key-{rng}", rng.Next()).ConfigureAwait(false);

        var after = pool.GetConnectionInformations();

        return BuildInfo(before) + "\t" + BuildInfo(after);

        static string BuildInfo(ConnectionPoolInformation info)
        {
            return $"\talive: {info.ActiveConnections.ToString()}, required: {info.RequiredPoolSize.ToString()}";
        }
    }
}
