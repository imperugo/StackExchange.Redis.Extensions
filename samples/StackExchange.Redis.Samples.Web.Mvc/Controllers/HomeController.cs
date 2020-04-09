using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Samples.Web.Mvc.Models;

namespace StackExchange.Redis.Samples.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IRedisDatabase redisDatabase;
        private readonly IRedisCacheConnectionPoolManager connectionPoolManager;

        public HomeController(ILogger<HomeController> logger, IRedisDatabase redisDatabase, IRedisCacheConnectionPoolManager connectionPoolManager)
        {
            this.logger = logger;
            this.redisDatabase = redisDatabase;
            this.connectionPoolManager = connectionPoolManager;
        }


        public async Task<IActionResult> Index()
        {
            var redisInfo = await redisDatabase.GetInfoAsync();
            var connectionInfo = connectionPoolManager.GetConnectionInformations();
            return Ok(new { RedisInfo = redisInfo, ConnectionInfo = connectionInfo });
        }
    }
}
